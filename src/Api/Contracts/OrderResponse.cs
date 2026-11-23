using Core.Domain;

namespace Api.Contracts;

// Вихідний DTO: те, що API віддає клієнту замість доменного Order.
public sealed record OrderLineResponse(string ProductId, string ProductName, decimal Price, int Quantity);

public sealed record OrderResponse(
    string Id,
    string Customer,
    DateTime CreatedAt,
    string Status,
    IReadOnlyList<OrderLineResponse> Lines,
    decimal Total);

// Варіант «Замовлення»: відповідь маршруту GET /api/orders/{id}/total.
public sealed record OrderTotalResponse(string Id, decimal Total, int LinesCount);

// Перетворення Order -> OrderResponse в одному місці.
public static class OrderMapping
{
    public static OrderResponse ToResponse(this Order order) =>
        new(order.Id,
            order.CustomerId,
            order.CreatedAt,
            order.Status.ToString(),
            order.Lines.Select(l => new OrderLineResponse(l.ProductId, l.ProductName, l.Price, l.Quantity)).ToList(),
            order.Total);
}