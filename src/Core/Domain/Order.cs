using Core.Dto;

namespace Core.Domain;

public sealed class Order
{
    private readonly List<OrderLine> _lines = [];

    public string Id { get; }
    public string CustomerId { get; }
    public bool IsConfirmed { get; private set; }

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
        // 1. Спочатку перевіряємо стан замовлення.
        if (IsConfirmed)
            throw new InvalidOperationException(
                $"Замовлення {Id} уже підтверджене, додавати рядки не можна");

        // 2. Потім створюємо рядок (перевіряє свої аргументи).
        var line = OrderLine.Create(productId, productName, price, quantity);

        // 3. І лише тепер змінюємо стан.
        _lines.Add(line);
    }

    public void Confirm()
    {
        if (IsConfirmed)
            throw new InvalidOperationException(
                $"Замовлення {Id} уже підтверджене");

        if (_lines.Count == 0)
            throw new InvalidOperationException(
                $"Замовлення {Id} порожнє, підтвердити його не можна");

        IsConfirmed = true;
    }

    // Сутність -> DTO (для збереження/виводу).
    public OrderDto ToDto() =>
        new(Id,
            CustomerId,
            IsConfirmed,
            _lines.Select(l => new OrderLineDto(l.ProductId, l.ProductName, l.Price, l.Quantity))
                  .ToList());

    // DTO -> сутність: лише через Create / AddLine / Confirm,
    // тож пошкоджені дані не створять некоректний об'єкт.
    public static Order FromDto(OrderDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(dto.Lines);

        var order = Create(dto.Id, dto.CustomerId);

        foreach (var line in dto.Lines)
            order.AddLine(line.ProductId, line.ProductName, line.Price, line.Quantity);

        if (dto.IsConfirmed)
            order.Confirm();

        return order;
    }
}
