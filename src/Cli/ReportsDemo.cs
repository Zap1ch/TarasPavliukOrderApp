using Core.Domain;
using Core.Reports;
using Core.Services;

// Лаба 7: команди звітів. Усі запити — у ReportService (Core), тут лише вибір команди і вивід.
public static class ReportsDemo
{
    public static void Run(OrderService service, string[] args)
    {
        // Назва звіту — перший аргумент, що починається з "report-" (--file сюди не потрапить).
        string command = args.FirstOrDefault(a => a.StartsWith("report-")) ?? "report-top";

        // З --empty звіти отримують порожній список (перевірка, що нічого не падає).
        IReadOnlyList<Order> items = args.Contains("--empty") ? new List<Order>() : service.All();

        switch (command)
        {
            case "report-top":
                Console.WriteLine("Топ-5 клієнтів за сумою замовлень:");
                PrintGroups(ReportService.TopCustomers(items, 5));
                break;

            case "report-expensive":
                Console.WriteLine("Замовлення дорожчі за 10000:");
                PrintOrders(ReportService.ExpensiveOrders(items, 10000m));
                break;

            case "report-groups":
                Console.WriteLine("Замовлення за статусом:");
                PrintGroups(ReportService.ByStatus(items));
                break;

            case "report-sum":
                var t = ReportService.Totals(items);
                Console.WriteLine($"Замовлень: {t.Orders}, загальна сума: {t.Total:F2}, " +
                                  $"середній чек: {t.Average:F2}, максимальне: {t.Max:F2}");
                break;

            case "report-deferred":
                DeferredDemo(service);
                break;

            default:
                Console.WriteLine("Команди: report-top | report-expensive | report-groups | report-sum | report-deferred");
                break;
        }
    }

    // Друкує підсумки груп: ключ, кількість замовлень, сума.
    private static void PrintGroups(IReadOnlyList<GroupSummary> groups)
    {
        if (groups.Count == 0)
            Console.WriteLine("  (порожньо)");

        foreach (var g in groups)
            Console.WriteLine($"  {g.Key,-10} замовлень: {g.Count,3}   сума: {g.TotalSum,10:F2}");
    }

    // Друкує рядки звіту про замовлення.
    private static void PrintOrders(IReadOnlyList<OrderReportLine> lines)
    {
        if (lines.Count == 0)
            Console.WriteLine("  (порожньо)");

        foreach (var l in lines)
            Console.WriteLine($"  {l.OrderId,-8} {l.CustomerId,-6} {l.Status,-10} рядків: {l.LinesCount}   сума: {l.Total,10:F2}");
    }

    // Крок 3. Відкладене виконання: запит виконується не тоді, коли написаний, а коли його перебирають.
    private static void DeferredDemo(OrderService service)
    {
        // Локальний список: service.All() щоразу дає нову копію, а нам треба змінювати саме джерело запиту.
        var source = service.All().ToList();

        var query = source.Where(o => o.Total > 10000m);   // запит побудовано, але НЕ виконано
        Console.WriteLine($"Дорогих (> 10000) до змін: {query.Count()}");

        // Додаємо в джерело ще одне дороге замовлення.
        var extra = Order.Create("tmp00001", "C-999");
        extra.AddLine("P-8", "Ноутбук", 28000m, 1);
        source.Add(extra);
        Console.WriteLine($"Дорогих (> 10000) після змін: {query.Count()}");   // число інше

        var snapshot = query.ToList();   // результат зафіксовано
        source.Clear();
        Console.WriteLine($"Джерело очищено, у ньому: {source.Count}");
        Console.WriteLine($"Запит тепер дає: {query.Count()}");
        Console.WriteLine($"У знімку лишилось: {snapshot.Count}");   // не змінюється
    }
}