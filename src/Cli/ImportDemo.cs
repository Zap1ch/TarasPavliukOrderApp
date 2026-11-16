using System.Diagnostics;
using System.Text.Json;
using Core;
using Core.Dto;
using Core.Services;
using Core.Storage;

// Лаба 8: асинхронний імпорт замовлень з JSON, скасування через Ctrl+C або тайм-аут.
public static class ImportDemo
{
    public static async Task<int> RunAsync(string[] args)
    {
        if (args.Contains("--gen-big"))
            return await GenerateBigAsync(args);

        // Файл після --import, інакше файл за замовчуванням.
        string path = ValueAfter(args, "--import") ?? Path.Combine("data", "import-orders.json");

        var service = new OrderService(StoreFactory.Create(args));

        // Подія з лаби 6: рахуємо, скільки замовлень реально додано.
        int events = 0;
        service.Changed += (_, _) => events++;

        // "Пульт" скасування: Ctrl+C або тайм-аут.
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;   // не вбивати процес одразу
            cts.Cancel();      // попросити імпорт зупинитися
        };

        string? timeoutText = ValueAfter(args, "--timeout");
        if (timeoutText is not null)
        {
            int ms = int.Parse(timeoutText);
            cts.CancelAfter(TimeSpan.FromMilliseconds(ms));
            Console.WriteLine($"Тайм-аут: {ms} мс");
        }

        Console.WriteLine($"Файл: {path}");
        Console.WriteLine($"У сховищі до імпорту: {service.All().Count}");
        Console.WriteLine("Імпорт... (Ctrl+C — скасувати)");

        var sw = Stopwatch.StartNew();
        int code = 0;

        // Порядок важливий: скасування — першим.
        try
        {
            int added = await service.ImportAsync(new JsonOrderImporter(), path, cts.Token);
            Console.WriteLine($"Імпортовано записів: {added} (за {sw.ElapsedMilliseconds} мс)");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Імпорт скасовано. (через {sw.ElapsedMilliseconds} мс)");
            code = 2;
        }
        catch (DomainException ex)
        {
            Console.WriteLine($"Дані відхилено: {ex.Message}");
            if (ex.InnerException is not null)
                Console.WriteLine($"  Початкова помилка: {ex.InnerException.GetType().Name}");
            code = 1;
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
            code = 1;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Файл пошкоджено, це не коректний JSON: {ex.Message}");
            code = 1;
        }

        Console.WriteLine($"Подій Changed: {events}");
        Console.WriteLine($"У сховищі після імпорту: {service.All().Count}");
        return code;
    }

    // Великий файл для демонстрації скасування (за замовчуванням 200 000 замовлень).
    private static async Task<int> GenerateBigAsync(string[] args)
    {
        int count = int.Parse(ValueAfter(args, "--gen-big") ?? "200000");
        string path = Path.Combine("data", "import-big.json");

        var dtos = new List<OrderDto>(count);
        for (int i = 1; i <= count; i++)
        {
            var lines = new List<OrderLineDto>
            {
                new("P-1", "Клавіатура", 1200m, 1 + i % 3),
                new("P-2", "Миша", 450.50m, 1 + i % 5),
                new("P-3", "Монітор", 5000m, 1)
            };
            dtos.Add(new OrderDto($"B-{i:D6}", $"C-{100 + i % 50}", "Confirmed", lines));
        }

        Directory.CreateDirectory("data");
        await using (var stream = File.Create(path))
        {
            await JsonSerializer.SerializeAsync(stream, dtos);
        }

        Console.WriteLine($"Створено {path}: {count} замовлень, {new FileInfo(path).Length / 1024 / 1024} МБ");
        return 0;
    }

    // Значення після аргументу: --timeout 500 -> "500".
    private static string? ValueAfter(string[] args, string name)
    {
        int i = Array.IndexOf(args, name);
        if (i >= 0 && i + 1 < args.Length && !args[i + 1].StartsWith("--"))
            return args[i + 1];
        return null;
    }
}