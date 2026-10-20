using Core.Dto;

namespace Core.Domain;

public sealed class Order
{
    private readonly List<OrderLine> _lines = [];

    public string Id { get; }
    public string CustomerId { get; }

    // Стан замовлення. Змінюється лише через Confirm і Cancel.
    public OrderStatus Status { get; private set; } = OrderStatus.Draft;

    // обчислюється зі статусу, присвоїти не можна.
    public bool IsConfirmed => Status == OrderStatus.Confirmed;

    // Назовні лише для читання: додати рядок можна тільки через AddLine.
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();

    public decimal Total => _lines.Sum(l => l.Total);

    private Order(string id, string customerId)
    {
        Id = id;
        CustomerId = customerId;
    }

    // Єдиний спосіб створити замовлення: усі перевірки тут.
    public static Order Create(string id, string customerId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Номер замовлення обов'язковий", nameof(id));

        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException(
                $"Замовлення {id.Trim()} має містити ідентифікатор покупця", nameof(customerId));

        return new Order(id.Trim(), customerId.Trim());
    }

    public void AddLine(string productId, string productName, decimal price, int quantity)
    {
        // 1. Спочатку перевіряю стан: рядки додаються лише до чернетки.
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException(
                $"Замовлення {Id} має статус {Status}, додавати рядки можна лише до чернетки (Draft)");

        // 2. Потім створюю рядок (перевіряє свої аргументи).
        var line = OrderLine.Create(productId, productName, price, quantity);

        // 3. І лише тепер змінюю стан.
        _lines.Add(line);
    }

    public void Confirm()
    {
        CheckTransition(OrderStatus.Confirmed);

        if (_lines.Count == 0)
            throw new InvalidOperationException(
                $"Замовлення {Id} порожнє, підтвердити його не можна");

        Status = OrderStatus.Confirmed;
    }

    public void Cancel()
    {
        CheckTransition(OrderStatus.Cancelled);
        Status = OrderStatus.Cancelled;
    }

    // Допустимі переходи між статусами (switch expression).
    // Усе, чого немає в списку, заборонено.
    public static bool CanTransition(OrderStatus from, OrderStatus to)
    {
        return (from, to) switch
        {
            (OrderStatus.Draft, OrderStatus.Confirmed) => true,
            (OrderStatus.Draft, OrderStatus.Cancelled) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
            _ => false
        };
    }

    private void CheckTransition(OrderStatus newStatus)
    {
        if (!CanTransition(Status, newStatus))
            throw new InvalidOperationException(
                $"Замовлення {Id}: перехід {Status} → {newStatus} недопустимий");
    }

    // Сутність -> DTO. Статус зберігаємо як рядок ("Draft", "Confirmed", "Cancelled").
    public OrderDto ToDto() =>
        new(Id,
            CustomerId,
            Status.ToString(),
            _lines.Select(l => new OrderLineDto(l.ProductId, l.ProductName, l.Price, l.Quantity))
                  .ToList());

    // DTO -> сутність: лише через Create / AddLine / Confirm / Cancel, тож пошкоджені дані не створять некоректний об'єкт.
    public static Order FromDto(OrderDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(dto.Lines);

        var order = Create(dto.Id, dto.CustomerId);
        OrderStatus status = ParseStatus(order.Id, dto.Status);

        foreach (var line in dto.Lines)
            order.AddLine(line.ProductId, line.ProductName, line.Price, line.Quantity);

        // Статус відновлюємо в кінці і через ті самі методи.
        if (status == OrderStatus.Confirmed)
            order.Confirm();
        if (status == OrderStatus.Cancelled)
            order.Cancel();

        return order;
    }

    // Рядок з файлу -> enum (теж switch expression). Невідомий статус відхиляємо.
    private static OrderStatus ParseStatus(string orderId, string status)
    {
        return status switch
        {
            "Draft" => OrderStatus.Draft,
            "Confirmed" => OrderStatus.Confirmed,
            "Cancelled" => OrderStatus.Cancelled,
            _ => throw new ArgumentException(
                $"Замовлення {orderId} має невідомий статус '{status}'", nameof(status))
        };
    }
}