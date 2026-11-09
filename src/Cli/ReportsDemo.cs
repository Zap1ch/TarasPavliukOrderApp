using Core.Domain;
using Core.Services;

// Лаба 7: команди звітів. Усі запити — у ReportService (Core), тут лише вибір команди і вивід.
public static class ReportsDemo
{
    public static void Run(OrderService service, string[] args)
    {
        // Назва звіту — перший аргумент, що починається з "report-".
        string command = args.First(a => a.StartsWith("report-"));

        switch (command)
        {
            case "report-deferred":
                DeferredDemo(service);
                break;

            default:
                Console.WriteLine("Команди: report-deferred");
                break;
        }
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