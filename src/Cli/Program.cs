using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Core.Import;
using Core.Services;
using Core.Storage;
using System.Text.Encodings.Web;
using System.Text.Json;

// Лаба 4: демо доменної моделі (окреме порожнє сховище в пам'яті).
if (args.Contains("--domain"))
{
    DomainDemo.Run(new OrderService(new InMemoryOrderStore()));
    return 0;
}

// Лаба 5: сервіс + два сховища.
if (args.Contains("--store") || args.Contains("--file"))
{
    // Composition root. Додаткове 3: вибір сховища винесено у фабрику.
    IOrderStore store = StoreFactory.Create(args);

    Console.WriteLine($"Сховище: {store.GetType().Name}");
    if (store is FileOrderStore)
        Console.WriteLine($"Файл: {StoreFactory.DataPath}");

    // Додаткове 1: декоратор поверх будь-якого сховища.
    CachingOrderStore? cache = null;
    if (args.Contains("--cache"))
    {
        cache = new CachingOrderStore(store);
        store = cache;
        Console.WriteLine("Обгорнуто в CachingOrderStore");
    }

    var service = new OrderService(store);

    // Додати і змінити: нове замовлення, два рядки, підтвердження.
    var created = service.CreateOrder("C-108");
    service.AddLine(created.Id, "P-5", "Кабель HDMI", 250m, 2);
    service.AddLine(created.Id, "P-3", "Монітор", 5000m, 1);
    service.Confirm(created.Id);
    Console.WriteLine($"Створено і підтверджено замовлення {created.Id}");

    // Показати список.
    Console.WriteLine();
    Console.WriteLine($"Усі замовлення ({service.All().Count}):");
    foreach (var o in service.All())
        Console.WriteLine($"  {o.Id,-8} {o.CustomerId,-6} {o.Status,-10} рядків: {o.Lines.Count}  сума: {o.Total,9:F2}");

    // Знайти за id.
    Console.WriteLine();
    var found = service.Find(created.Id);
    if (found is null)
        Console.WriteLine($"Замовлення {created.Id} не знайдено");
    else
        Console.WriteLine($"Знайдено {found.Id}: покупець {found.CustomerId}, статус {found.Status}, сума {found.Total:F2}");

    // Додаткове 2: пошук за умовою (Func<Order, bool>).
    if (args.Contains("--search"))
    {
        PrintOrders("Чернетки", service.Search(o => o.Status == OrderStatus.Draft));
        PrintOrders("Замовлення покупця C-100", service.Search(o => o.CustomerId == "C-100"));
        PrintOrders("Сума понад 10000", service.Search(o => o.Total > 10000m));
    }

    // Сценарій відмови: Core кидає виняток, Cli перехоплює і пише зрозумілий текст.
    Console.WriteLine();
    Console.WriteLine("Сценарій відмови:");

    try
    {
        // Дубль: замовлення з таким номером уже є.
        store.Add(Order.Create(created.Id, "C-108"));
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"  Не вдалося додати замовлення: {ex.Message}");
    }

    try
    {
        // Неіснуючий номер.
        service.Confirm("O-999");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"  Не вдалося підтвердити замовлення: {ex.Message}");
    }

    // Додаткове 1: статистика кешу.
    if (cache is not null)
    {
        Console.WriteLine();
        Console.WriteLine($"Кеш: відповідей з кешу {cache.Hits}, звернень до сховища {cache.Misses}");
    }

    return 0;

    // Друкує заголовок і знайдені замовлення.
    static void PrintOrders(string title, IReadOnlyList<Order> orders)
    {
        Console.WriteLine();
        Console.WriteLine($"{title} ({orders.Count}):");
        foreach (var o in orders)
            Console.WriteLine($"  {o.Id,-8} {o.CustomerId,-6} {o.Status,-10} сума: {o.Total,9:F2}");
    }
}

// Лаба 3: імпорт CSV / JSON.
bool jsonOutput = args.Contains("--json");
bool mixedMode = args.Contains("--mixed");

string? pathArgument =
    args.FirstOrDefault(arg => !arg.StartsWith("--"));

string path = pathArgument
    ?? (mixedMode
        ? Path.Combine("data", "sample-mixed.csv")
        : Path.Combine("data", "sample.csv"));

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};

if (mixedMode)
{
    MixedImportResult mixedResult =
        MixedCsvImporter.Load(path);

    if (jsonOutput)
    {
        Console.WriteLine(
            JsonSerializer.Serialize(mixedResult, jsonOptions));

        return 0;
    }

    Console.WriteLine(
        $"Завантажено товарів: {mixedResult.Products.Count}");

    foreach (ProductDto product in mixedResult.Products.Take(5))
    {
        Console.WriteLine(
            $"P {product.Id,-7} {product.Name,-25} {product.Price,10:F2}");
    }

    Console.WriteLine();

    Console.WriteLine(
        $"Завантажено складів: {mixedResult.Warehouses.Count}");

    foreach (WarehouseDto warehouse in mixedResult.Warehouses.Take(5))
    {
        Console.WriteLine(
            $"W {warehouse.Id,-7} {warehouse.Name,-25} {warehouse.City}");
    }

    if (mixedResult.Errors.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine(
            $"Пропущено рядків: {mixedResult.Errors.Count}");

        foreach (string error in mixedResult.Errors)
        {
            Console.WriteLine($"! {error}");
        }
    }

    int accepted =
        mixedResult.Products.Count +
        mixedResult.Warehouses.Count;

    int total =
        accepted + mixedResult.Errors.Count;

    double errorPercent =
        total == 0
            ? 0
            : mixedResult.Errors.Count * 100.0 / total;

    Console.WriteLine();
    Console.WriteLine(
        $"Статистика: усього {total}, прийнято {accepted}, " +
        $"пропущено {mixedResult.Errors.Count}, помилок {errorPercent:F1}%");

    return 0;
}

ImportResult<ProductDto> result =
    Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".csv" => ProductCsvImporter.Load(path),
        ".json" => ProductJsonImporter.Load(path),

        _ => new ImportResult<ProductDto>(
            [],
            [$"Непідтримуваний формат файлу: {Path.GetExtension(path)}"])
    };

if (jsonOutput)
{
    Console.WriteLine(
        JsonSerializer.Serialize(result, jsonOptions));

    return 0;
}

Console.WriteLine($"Завантажено записів: {result.Items.Count}");

foreach (ProductDto product in result.Items.Take(5))
{
    Console.WriteLine(
        $"{product.Id,-7} {product.Name,-30} {product.Price,10:F2}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine();
    Console.WriteLine(
        $"Пропущено рядків: {result.Errors.Count}");

    foreach (string error in result.Errors)
    {
        Console.WriteLine($"! {error}");
    }
}

int acceptedCount = result.Items.Count;
int totalCount = acceptedCount + result.Errors.Count;

double errorPercentage =
    totalCount == 0
        ? 0
        : result.Errors.Count * 100.0 / totalCount;

Console.WriteLine();
Console.WriteLine(
    $"Статистика: усього {totalCount}, прийнято {acceptedCount}, " +
    $"пропущено {result.Errors.Count}, помилок {errorPercentage:F1}%");

return 0;