using System.Text.Encodings.Web;
using System.Text.Json;
using Core.Domain;
using Core.Dto;
using Core.Services;

public static class DomainDemo
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    // Сервіс приходить з Program.cs (там створюється сховище).
    public static void Run(OrderService service)
    {
        Console.WriteLine("Сценарій 1: коректне замовлення");
        var order = Order.Create("O-001", "C-100");
        order.AddLine("P-1", "Клавіатура", 1200m, 1);
        order.AddLine("P-2", "Миша", 450.50m, 2);
        order.Confirm();
        Print(order);

        Console.WriteLine();
        Console.WriteLine("Сценарій 2: порушення інваріантів");

        Try("1. Порожній номер замовлення",
            () => Order.Create("", "C-100"));

        Try("2. Порожній покупець",
            () => Order.Create("O-002", " "));

        var draft = Order.Create("O-003", "C-100");
        draft.AddLine("P-1", "Клавіатура", 1200m, 1);

        Try("3. Порожній ID товару",
            () => draft.AddLine(" ", "Миша", 450m, 1));

        Try("4. Порожня назва товару",
            () => draft.AddLine("P-2", "", 450m, 1));

        Try("5. Нульова кількість",
            () => draft.AddLine("P-2", "Миша", 450m, 0));

        Try("6. Від'ємна ціна",
            () => draft.AddLine("P-2", "Миша", -5m, 1));

        Try("7. Підтвердження порожнього замовлення",
            () => Order.Create("O-004", "C-100").Confirm());

        Try("8. Рядок у підтверджене замовлення",
            () => order.AddLine("P-3", "Монітор", 5000m, 1));

        Try("9. Повторне підтвердження",
            () => order.Confirm());

        Try("10. Пошкоджений DTO (підтверджене без рядків)",
            () => Order.FromDto(new OrderDto("O-005", "C-100", "Confirmed", new List<OrderLineDto>())));

        Console.WriteLine();
        Console.WriteLine("Стан після відмов (не змінився)");
        Print(order);
        Print(draft);

        Console.WriteLine();
        Console.WriteLine("Сценарій 3: ToDto → JSON → FromDto");

        // Зберігаємо сутність у формат DTO і серіалізуємо в JSON, як у лабі 3.
        string json = JsonSerializer.Serialize(order.ToDto(), JsonOptions);
        Console.WriteLine(json);

        // Відновлюємо: JSON → DTO → сутність (через ті самі перевірки).
        OrderDto loaded = JsonSerializer.Deserialize<OrderDto>(json, JsonOptions)!;
        Order restored = Order.FromDto(loaded);
        Console.Write("Відновлено: ");
        Print(restored);

        // Той самий JSON, але з пошкодженим рядком: кількість 0.
        string broken = json.Replace("\"Quantity\": 2", "\"Quantity\": 0");
        Try("11. Пошкоджений JSON (кількість 0)",
            () => Order.FromDto(JsonSerializer.Deserialize<OrderDto>(broken, JsonOptions)!));

        Console.WriteLine();
        Console.WriteLine("Додаткове 1: ImportResult → сутності + помилки");

        // Ніби дані, які прийшли з файлу: 2 нормальні рядки і 2 з порушеннями.
        ImportResult<OrderLineDto> import = new ImportResult<OrderLineDto>(
            new List<OrderLineDto>
            {
                new OrderLineDto("P-1", "Клавіатура", 1200m, 1),
                new OrderLineDto("P-2", "Миша", -10m, 1),
                new OrderLineDto("P-3", "Монітор", 5000m, 0),
                new OrderLineDto("P-4", "Навушники", 900m, 2)
            },
            new List<string>());

        ImportResult<OrderLine> result = OrderLineImport.ToEntities(import);

        Console.WriteLine($"Прийнято рядків: {result.Items.Count}, з помилками: {result.Errors.Count}");
        foreach (OrderLine l in result.Items)
            Console.WriteLine($"  + {l.ProductId} {l.ProductName}: {l.Price} x {l.Quantity} = {l.Total}");
        foreach (string error in result.Errors)
            Console.WriteLine($"  ! {error}");

        Console.WriteLine();
        Console.WriteLine("Додаткове 2: не більше 5 непідтверджених замовлень у покупця");

        // З лаби 5 список замовлень замінило сховище, сервіс працює через нього.
        List<Order> created = new List<Order>();
        for (int i = 1; i <= 5; i++)
            created.Add(service.CreateOrder("C-200"));

        Console.WriteLine($"У покупця C-200 непідтверджених замовлень: {service.CountDrafts("C-200")}");

        Try("12. Шосте непідтверджене замовлення",
            () => service.CreateOrder("C-200"));

        // Підтверджуємо одне замовлення, і місце звільняється.
        service.AddLine(created[0].Id, "P-1", "Клавіатура", 1200m, 1);
        service.Confirm(created[0].Id);
        Order sixth = service.CreateOrder("C-200");
        Console.WriteLine($"Після підтвердження {created[0].Id} створено {sixth.Id}, " +
                          $"непідтверджених: {service.CountDrafts("C-200")}");

        // Інших покупців це правило не зачіпає.
        Order other = service.CreateOrder("C-300");
        Console.WriteLine($"Покупець C-300: створено {other.Id}");

        Console.WriteLine();
        Console.WriteLine("Додаткове 3: статуси замовлення");

        Console.WriteLine("Допустимі переходи (CanTransition):");
        Console.WriteLine($"  Draft → Confirmed:     {Order.CanTransition(OrderStatus.Draft, OrderStatus.Confirmed)}");
        Console.WriteLine($"  Draft → Cancelled:     {Order.CanTransition(OrderStatus.Draft, OrderStatus.Cancelled)}");
        Console.WriteLine($"  Confirmed → Cancelled: {Order.CanTransition(OrderStatus.Confirmed, OrderStatus.Cancelled)}");
        Console.WriteLine($"  Confirmed → Draft:     {Order.CanTransition(OrderStatus.Confirmed, OrderStatus.Draft)}");
        Console.WriteLine($"  Cancelled → Draft:     {Order.CanTransition(OrderStatus.Cancelled, OrderStatus.Draft)}");
        Console.WriteLine($"  Cancelled → Confirmed: {Order.CanTransition(OrderStatus.Cancelled, OrderStatus.Confirmed)}");

        // Чернетку скасовуємо.
        Order cancelled = Order.Create("O-301", "C-400");
        cancelled.AddLine("P-4", "Навушники", 900m, 1);
        cancelled.Cancel();
        Print(cancelled);

        // Підтверджене теж можна скасувати.
        Order confirmedThenCancelled = Order.Create("O-302", "C-400");
        confirmedThenCancelled.AddLine("P-1", "Клавіатура", 1200m, 1);
        confirmedThenCancelled.Confirm();
        confirmedThenCancelled.Cancel();
        Print(confirmedThenCancelled);

        Try("13. Підтвердження скасованого",
            () => cancelled.Confirm());

        Try("14. Повторне скасування",
            () => cancelled.Cancel());

        Try("15. Рядок у скасоване замовлення",
            () => cancelled.AddLine("P-5", "Кабель", 100m, 1));

        Try("16. DTO з невідомим статусом",
            () => Order.FromDto(new OrderDto("O-303", "C-400", "Shipped",
                new List<OrderLineDto> { new OrderLineDto("P-1", "Клавіатура", 1200m, 1) })));

        // Статус зберігається і відновлюється через ToDto / FromDto.
        Order restoredCancelled = Order.FromDto(cancelled.ToDto());
        Console.Write("Відновлено з DTO: ");
        Print(restoredCancelled);
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
            Console.WriteLine($"{title}: {ex.GetType().Name} — {ex.Message}");
        }
    }

    private static void Print(Order o)
    {
        Console.WriteLine($"Замовлення {o.Id}, покупець {o.CustomerId}, " +
                          $"статус: {o.Status}, рядків: {o.Lines.Count}, сума: {o.Total}");
        foreach (var l in o.Lines)
            Console.WriteLine($"  - {l.ProductId} {l.ProductName}: {l.Price} x {l.Quantity} = {l.Total}");
    }
}