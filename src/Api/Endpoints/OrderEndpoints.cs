using Api.Contracts;
using Core.Abstractions;
using Core.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

// Лаба 10: маршрути /api/orders — список, одне, створити, замінити, видалити.
public static class OrderEndpoints
{
    public static void MapOrders(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        // GET /api/orders — усі замовлення.
        group.MapGet("/", (IOrderStore store) =>
            TypedResults.Ok(store.List().Select(o => o.ToResponse()).ToList()));

        // GET /api/orders/{id} — 200 або 404.
        group.MapGet("/{id}", Results<Ok<OrderResponse>, NotFound> (string id, IOrderStore store) =>
            store.GetById(id) is { } found
                ? TypedResults.Ok(found.ToResponse())
                : TypedResults.NotFound());

        // POST /api/orders — 201 з адресою нового замовлення або 400 зі списком помилок.
        group.MapPost("/", Results<Created<OrderResponse>, ValidationProblem> (SaveOrderRequest request, IOrderStore store) =>
        {
            if (!request.TryValidate(out var errors))
                return TypedResults.ValidationProblem(errors);

            var id = Guid.NewGuid().ToString("N")[..8];
            var created = BuildOrder(id, request);
            store.Add(created);
            return TypedResults.Created($"/api/orders/{created.Id}", created.ToResponse());
        });

        // PUT /api/orders/{id} — повна заміна: 200, 404 або 400. Дата створення лишається стара.
        group.MapPut("/{id}", Results<Ok<OrderResponse>, NotFound, ValidationProblem> (string id, SaveOrderRequest request, IOrderStore store) =>
        {
            if (!request.TryValidate(out var errors))
                return TypedResults.ValidationProblem(errors);

            var existing = store.GetById(id);
            if (existing is null)
                return TypedResults.NotFound();

            var replacement = BuildOrder(id, request, existing.CreatedAt);
            store.Update(replacement);
            return TypedResults.Ok(replacement.ToResponse());
        });

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