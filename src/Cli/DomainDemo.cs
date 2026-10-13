using Core.Domain;
using Core.Dto;

public static class DomainDemo
{
    public static void Run()
    {
        Console.WriteLine("=== Сценарій 1: коректне замовлення ===");
        var order = Order.Create("O-001", "C-100");
        order.AddLine("P-1", "Клавіатура", 1200m, 1);
        order.AddLine("P-2", "Миша", 450.50m, 2);
        order.Confirm();
        Print(order);

        Console.WriteLine();
        Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");

        Try("1. Порожній номер замовлення",
            () => Order.Create("", "C-100"));

        Try("2. Порожній покупець",
            () => Order.Create("O-002", " "));

        var draft = Order.Create("O-003", "C-100");
        draft.AddLine("P-1", "Клавіатура", 1200m, 1);

        Try("3. Нульова кількість",
            () => draft.AddLine("P-2", "Миша", 450m, 0));

        Try("4. Від'ємна ціна",
            () => draft.AddLine("P-2", "Миша", -5m, 1));

        Try("5. Підтвердження порожнього замовлення",
            () => Order.Create("O-004", "C-100").Confirm());

        Try("6. Рядок у підтверджене замовлення",
            () => order.AddLine("P-3", "Монітор", 5000m, 1));

        Try("7. Повторне підтвердження",
            () => order.Confirm());

        Try("8. Пошкоджений DTO (підтверджене без рядків)",
            () => Order.FromDto(new OrderDto("O-005", "C-100", true, new List<OrderLineDto>())));

        Console.WriteLine();
        Console.WriteLine("=== Стан після відмов (не змінився) ===");
        Print(order);
        Print(draft);
    }

    private static void Try(string title, Action action)
    {
        try
        {
            action();
            Console.WriteLine($"{title}: ПОМИЛКА, виняток не виник!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{title}: {ex.Message}");
        }
    }

    private static void Print(Order o)
    {
        Console.WriteLine($"Замовлення {o.Id}, покупець {o.CustomerId}, " +
                          $"підтверджене: {o.IsConfirmed}, рядків: {o.Lines.Count}, сума: {o.Total}");
        foreach (var l in o.Lines)
            Console.WriteLine($"  - {l.ProductId} {l.ProductName}: {l.Price} x {l.Quantity} = {l.Total}");
    }
}
