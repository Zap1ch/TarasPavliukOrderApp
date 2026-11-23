using Api.Contracts;
using Api.Validation;
using Core.Abstractions;
using Core.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

// Лаба 10: маршрути /api/orders — список, одне, створити, замінити, видалити, сума.
public static class OrderEndpoints
{
    public static void MapOrders(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        // GET /api/orders?page=1&pageSize=20&search=... — список сторінками { items, total }.
        group.MapGet("/", (IOrderStore store, int page = 1, int pageSize = 20, string? search = null) =>
        {
            // Захист від некоректних значень.
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 1;
            if (pageSize > 100) pageSize = 100;

            IEnumerable<Order> orders = store.List();

            // Пошук за номером, покупцем або назвою товару (без урахування регістру).
            if (!string.IsNullOrWhiteSpace(search))
            {
                orders = orders.Where(o =>
                    o.Id.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    o.CustomerId.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    o.Lines.Any(l => l.ProductName.Contains(search, StringComparison.OrdinalIgnoreCase)));
            }

            // Сортую за номером, щоб сторінки були стабільні.
            var filtered = orders.OrderBy(o => o.Id).ToList();

            // Беру лише потрібну сторінку.
            var items = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => o.ToResponse())
                .ToList();

            return TypedResults.Ok(new PagedResponse<OrderResponse>(items, filtered.Count));
        });

        // GET /api/orders/{id} — 200 або 404.
        group.MapGet("/{id}", Results<Ok<OrderResponse>, NotFound> (string id, IOrderStore store) =>
            store.GetById(id) is { } found
                ? TypedResults.Ok(found.ToResponse())
                : TypedResults.NotFound());

        // GET /api/orders/{id}/total — варіант «Замовлення»: сума і кількість рядків, 200 або 404.
        group.MapGet("/{id}/total", Results<Ok<OrderTotalResponse>, NotFound> (string id, IOrderStore store) =>
            store.GetById(id) is { } found
                ? TypedResults.Ok(new OrderTotalResponse(found.Id, found.Total, found.Lines.Count))
                : TypedResults.NotFound());

        // POST /api/orders — 201 з адресою нового замовлення.
        // 400 зі списком помилок повертає ValidationFilter (FluentValidation) ще до цього коду.
        group.MapPost("/", (SaveOrderRequest request, IOrderStore store) =>
        {
            var id = Guid.NewGuid().ToString("N")[..8];
            var created = BuildOrder(id, request);
            store.Add(created);
            return TypedResults.Created($"/api/orders/{created.Id}", created.ToResponse());
        })
        .AddEndpointFilter<ValidationFilter<SaveOrderRequest>>()
        .ProducesValidationProblem();

        // PUT /api/orders/{id} — повна заміна: 200, 404 або 409 (не чернетка), 400 — з ValidationFilter.
        // Дата створення лишається стара.
        group.MapPut("/{id}", Results<Ok<OrderResponse>, NotFound, ProblemHttpResult> (string id, SaveOrderRequest request, IOrderStore store) =>
        {
            var existing = store.GetById(id);
            if (existing is null)
                return TypedResults.NotFound();

            // Правило лаби 4: змінювати рядки можна лише в чернетці -> 409 Conflict, а не 500.
            if (existing.Status != OrderStatus.Draft)
                return TypedResults.Problem(
                    title: "Замовлення не можна змінити",
                    detail: $"Замовлення {id} має статус {existing.Status}, замінити можна лише чернетку (Draft).",
                    statusCode: StatusCodes.Status409Conflict);

            var replacement = BuildOrder(id, request, existing.CreatedAt);
            store.Update(replacement);
            return TypedResults.Ok(replacement.ToResponse());
        })
        .AddEndpointFilter<ValidationFilter<SaveOrderRequest>>()
        .ProducesValidationProblem();

        // POST /api/orders/{id}/confirm — підтвердити: 200 або 404.
        // Статус тут не перевіряю: недопустимий перехід -> Order кидає DomainException -> DomainExceptionHandler дає 409.
        group.MapPost("/{id}/confirm", Results<Ok<OrderResponse>, NotFound> (string id, IOrderStore store) =>
        {
            var order = store.GetById(id);
            if (order is null)
                return TypedResults.NotFound();

            order.Confirm();
            store.Update(order);
            return TypedResults.Ok(order.ToResponse());
        })
        .ProducesProblem(StatusCodes.Status409Conflict);

        // POST /api/orders/{id}/cancel — скасувати: 200, 404 або 409 (через DomainException).
        group.MapPost("/{id}/cancel", Results<Ok<OrderResponse>, NotFound> (string id, IOrderStore store) =>
        {
            var order = store.GetById(id);
            if (order is null)
                return TypedResults.NotFound();

            order.Cancel();
            store.Update(order);
            return TypedResults.Ok(order.ToResponse());
        })
        .ProducesProblem(StatusCodes.Status409Conflict);

        // DELETE /api/orders/{id} — 204 або 404.
        group.MapDelete("/{id}", Results<NoContent, NotFound> (string id, IOrderStore store) =>
            store.Remove(id) ? TypedResults.NoContent() : TypedResults.NotFound());
    }

    // Нове замовлення-чернетка з рядками із запиту — через правила лаби 4.
    private static Order BuildOrder(string id, SaveOrderRequest request, DateTime? createdAt = null)
    {
        var order = Order.Create(id, request.Customer, createdAt);
        foreach (var line in request.Lines)
            order.AddLine(line.ProductId, line.ProductName, line.Price, line.Quantity);
        return order;
    }
}