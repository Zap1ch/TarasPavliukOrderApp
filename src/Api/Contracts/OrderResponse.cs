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