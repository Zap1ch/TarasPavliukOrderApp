using Core.Domain;

namespace Core.Services;

// Додаткове 2: правило на ДВІ сутності (покупець і його замовлення).
// Одне замовлення не знає про інші замовлення покупця, тому це правило перевіряє сервіс, а не клас Order.
public static class OrderService
{
    public const int MaxDraftOrders = 5;

    // Скільки непідтверджених замовлень має покупець.
    public static int CountDrafts(string customerId, IReadOnlyList<Order> orders)
    {
        int count = 0;
        foreach (Order order in orders)
        {
            if (order.CustomerId == customerId && !order.IsConfirmed)
                count++;
        }
        return count;
    }

    // Створює нове замовлення, якщо в покупця менше 5 непідтверджених.
    public static Order CreateOrder(string id, string customerId, IReadOnlyList<Order> orders)
    {
        // 1. Фабрика перевіряє самі аргументи (порожній номер, порожній покупець).
        Order newOrder = Order.Create(id, customerId);

        // 2. Потім правило на кілька сутностей.
        int drafts = CountDrafts(newOrder.CustomerId, orders);
        if (drafts >= MaxDraftOrders)
            throw new InvalidOperationException(
                $"Покупець {newOrder.CustomerId} вже має {drafts} непідтверджених замовлень, " +
                $"нове замовлення {newOrder.Id} створити не можна");

        return newOrder;
    }
}