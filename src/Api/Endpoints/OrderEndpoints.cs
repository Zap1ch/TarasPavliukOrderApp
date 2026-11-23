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

        // POST /api/orders — номер дає сервер, відповідь 201 з адресою нового замовлення.
        group.MapPost("/", (SaveOrderRequest request, IOrderStore store) =>
        {
            var id = Guid.NewGuid().ToString("N")[..8];
            var created = BuildOrder(id, request);
            store.Add(created);
            return TypedResults.Created($"/api/orders/{created.Id}", created.ToResponse());
        });

        // PUT /api/orders/{id} — повна заміна: 200 або 404.
        group.MapPut("/{id}", Results<Ok<OrderResponse>, NotFound> (string id, SaveOrderRequest request, IOrderStore store) =>
        {
            if (store.GetById(id) is null)
                return TypedResults.NotFound();

            var replacement = BuildOrder(id, request);
            store.Update(replacement);
            return TypedResults.Ok(replacement.ToResponse());
        });

        // DELETE /api/orders/{id} — 204 або 404.
        group.MapDelete("/{id}", Results<NoContent, NotFound> (string id, IOrderStore store) =>
            store.Remove(id) ? TypedResults.NoContent() : TypedResults.NotFound());
    }

    // Нове замовлення-чернетка з рядками із запиту — через правила лаби 4.
    private static Order BuildOrder(string id, SaveOrderRequest request)
    {
        var order = Order.Create(id, request.Customer);
        foreach (var line in request.Lines)
            order.AddLine(line.ProductId, line.ProductName, line.Price, line.Quantity);
        return order;
    }
}