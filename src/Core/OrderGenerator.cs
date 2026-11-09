using Core.Domain;

namespace Core;

// Додаткове 4 (лаба 7): генератор великої кількості замовлень для виміру швидкості звітів.
public static class OrderGenerator
{
    public static List<Order> Generate(int count)
    {
        var random = new Random(42);   // фіксоване зерно — однакові дані при кожному запуску
        var orders = new List<Order>(count);

        for (int i = 1; i <= count; i++)
        {
            var order = Order.Create($"G-{i:D6}", $"C-{random.Next(100, 200)}");
            order.AddLine("P-1", "Товар", random.Next(100, 30000), random.Next(1, 5));

            if (random.Next(3) > 0)   // приблизно 2/3 замовлень підтверджені
                order.Confirm();

            orders.Add(order);
        }

        return orders;
    }
}