using Api.Contracts;
using Core.Abstractions;
using Core.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

// Усі маршрути для замовлень: /api/orders
public static class OrderEndpoints
{
    public static void MapOrders(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        // GET /api/orders?page=1&pageSize=20&search=... — список сторінками
        group.MapGet("/", (IOrderStore store, int page = 1, int pageSize = 20, string? search = null) =>
        {
            // захист від некоректних значень
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 1;
            if (pageSize > 100) pageSize = 100;

            IEnumerable<Order> orders = store.List();

            // пошук за номером, покупцем або назвою товару (без урахування регістру)
            if (!string.IsNullOrWhiteSpace(search))
            {
                orders = orders.Where(o =>
                    o.Id.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    o.CustomerId.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    o.Lines.Any(l => l.ProductName.Contains(search, StringComparison.OrdinalIgnoreCase)));
            }

            // сортую за номером, щоб сторінки були стабільні
            var filtered = orders.OrderBy(o => o.Id).ToList();

            // беру лише потрібну сторінку
            var items = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => o.ToResponse())
                .ToList();

            return TypedResults.Ok(new PagedResponse<OrderResponse>(items, filtered.Count));
        });

        // GET /api/orders/{id} — одне замовлення
        group.MapGet("/{id}", Results<Ok<OrderResponse>, NotFound> (string id, IOrderStore store) =>
        {
            var order = store.GetById(id);
            if (order is null)
                return TypedResults.NotFound();

            return TypedResults.Ok(order.ToResponse());
        });

        // GET /api/orders/{id}/total — лише сума замовлення
        group.MapGet("/{id}/total", Results<Ok<OrderTotalResponse>, NotFound> (string id, IOrderStore store) =>
        {
            var order = store.GetById(id);
            if (order is null)
                return TypedResults.NotFound();

            return TypedResults.Ok(new OrderTotalResponse(order.Id, order.Total, order.Lines.Count));
        });

        // POST /api/orders — створити нове замовлення
        group.MapPost("/", Results<Created<OrderResponse>, ValidationProblem> (SaveOrderRequest request, IOrderStore store) =>
        {
            // спершу перевіряю вхідні дані
            if (!request.TryValidate(out var errors))
                return TypedResults.ValidationProblem(errors);

            // короткий унікальний номер
            var id = Guid.NewGuid().ToString("N")[..8];
            var order = BuildOrder(id, request);
            store.Add(order);

            return TypedResults.Created($"/api/orders/{id}", order.ToResponse());
        });

        // PUT /api/orders/{id} — повністю замінити замовлення
        group.MapPut("/{id}", Results<Ok<OrderResponse>, NotFound, ValidationProblem, ProblemHttpResult> (string id, SaveOrderRequest request, IOrderStore store) =>
        {
            if (!request.TryValidate(out var errors))
                return TypedResults.ValidationProblem(errors);

            var existing = store.GetById(id);
            if (existing is null)
                return TypedResults.NotFound();

            // змінювати можна лише чернетку
            if (existing.Status != OrderStatus.Draft)
            {
                return TypedResults.Problem(
                    title: "Замовлення не можна змінити",
                    detail: $"Замовлення {id} має статус {existing.Status}, замінити можна лише чернетку (Draft).",
                    statusCode: StatusCodes.Status409Conflict);
            }

            // дату створення лишаю стару
            var updated = BuildOrder(id, request, existing.CreatedAt);
            store.Update(updated);

            return TypedResults.Ok(updated.ToResponse());
        });

        // POST /api/orders/{id}/confirm — підтвердити замовлення
        // статус не перевіряю: якщо перехід недопустимий, Order кине DomainException → обробник дасть 409
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

        // POST /api/orders/{id}/cancel — скасувати замовлення
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

        // DELETE /api/orders/{id} — видалити замовлення
        group.MapDelete("/{id}", Results<NoContent, NotFound> (string id, IOrderStore store) =>
        {
            if (!store.Remove(id))
                return TypedResults.NotFound();

            return TypedResults.NoContent();
        });
    }

    // Збирає доменне замовлення з DTO запиту (інваріанти перевіряє сам Order)
    private static Order BuildOrder(string id, SaveOrderRequest request, DateTime? createdAt = null)
    {
        var order = Order.Create(id, request.Customer, createdAt);

        foreach (var line in request.Lines)
            order.AddLine(line.ProductId, line.ProductName, line.Price, line.Quantity);

        return order;
    }
}