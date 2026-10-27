using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

// Бізнес-операції із замовленнями. Знає лише інтерфейс сховища, а не пам'ять чи файл.
public sealed class OrderService(IOrderStore store)
{
    public const int MaxDraftOrders = 5;

    private readonly IOrderStore _store = store ?? throw new ArgumentNullException(nameof(store));

    public Order CreateOrder(string customerId)
    {
        // Короткий унікальний номер: перші 8 символів Guid.
        var order = Order.Create(Guid.NewGuid().ToString("N")[..8], customerId);

        // Правило на кілька замовлень (з лаби 4): не більше 5 чернеток у покупця.
        int drafts = CountDrafts(order.CustomerId);
        if (drafts >= MaxDraftOrders)
            throw new InvalidOperationException(
                $"Покупець {order.CustomerId} вже має {drafts} непідтверджених замовлень, нове створити не можна.");

        _store.Add(order);
        return order;
    }

    public void AddLine(string orderId, string productId, string productName, decimal price, int quantity)
    {
        var order = GetOrThrow(orderId);
        order.AddLine(productId, productName, price, quantity); // інваріанти перевіряє Order
        _store.Update(order);
    }

    public void Confirm(string orderId)
    {
        var order = GetOrThrow(orderId);
        order.Confirm();
        _store.Update(order);
    }

    public IReadOnlyList<Order> All() => _store.List();

    public Order? Find(string id) => _store.GetById(id);

    public int CountDrafts(string customerId) =>
        _store.List().Count(o => o.CustomerId == customerId && o.Status == OrderStatus.Draft);

    // Додаткове 2: пошук за будь-якою умовою. Умову передає той, хто викликає.
    public IReadOnlyList<Order> Search(Func<Order, bool> filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var result = new List<Order>();
        foreach (var order in _store.List())
        {
            if (filter(order))
                result.Add(order);
        }
        return result;
    }

    // Відсутнє замовлення тут — помилка, а не нормальний результат.
    private Order GetOrThrow(string id) =>
        _store.GetById(id) ?? throw new InvalidOperationException($"Немає замовлення з id={id}.");
}