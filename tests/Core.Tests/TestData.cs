using Core.Domain;

namespace Core.Tests;

// Маленький фіксований набір для тестів звітів (не SampleData — щоб тести не ламалися від змін демо-даних).
public static class TestData
{
    // Суми: T-1 1000, T-2 3000, T-3 3000, T-4 500, T-5 200.
    // Покупці: C-2 = 3000, C-3 = 3000 (нічия!), C-1 = 1500, C-4 = 200.
    public static IReadOnlyList<Order> Orders() => new List<Order>
    {
        Make("T-1", "C-1", 1000m),
        Make("T-2", "C-2", 3000m),
        Make("T-3", "C-3", 3000m),
        Make("T-4", "C-1", 500m),
        Make("T-5", "C-4", 200m)
    };

    // Підтверджене замовлення з одним рядком на задану суму.
    private static Order Make(string id, string customerId, decimal total)
    {
        var order = Order.Create(id, customerId);
        order.AddLine("P-1", "Товар", total, 1);
        order.Confirm();
        return order;
    }
}