namespace Core.Domain;

public sealed class OrderLine
{
    public string ProductId { get; }
    public string ProductName { get; }
    public decimal Price { get; }
    public int Quantity { get; }

    public decimal Total => Price * Quantity;

    private OrderLine(string productId, string productName, decimal price, int quantity)
    {
        ProductId = productId;
        ProductName = productName;
        Price = price;
        Quantity = quantity;
    }

    // Єдиний спосіб створити рядок: усі перевірки тут, до створення об'єкта.
    public static OrderLine Create(string productId, string productName, decimal price, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException(
                "Ідентифікатор товару в рядку замовлення обов'язковий", nameof(productId));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException(
                $"Назва товару {productId} в рядку замовлення не може бути порожньою", nameof(productName));

        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), price,
                $"Ціна товару {productId} не може бути від'ємною");

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity,
                $"Кількість товару {productId} у рядку має бути більшою за нуль");

        return new OrderLine(productId.Trim(), productName.Trim(), price, quantity);
    }
}
