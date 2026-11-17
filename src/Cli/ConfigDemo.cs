using System.Text.Json;
using Core;
using Core.Storage;

// Лаба 9: показ налаштувань, експорт та імпорт усієї колекції в JSON.
public static class ConfigDemo
{
    public static int Run(string[] args)
    {
        var config = AppConfig.Build(args);
        var options = AppConfig.Options(config);
        string dataFile = AppConfig.DataFile(options);

        Console.WriteLine($"Середовище : {options.EnvironmentName}");
        Console.WriteLine($"Рівень логу: {options.Logging.MinimumLevel}");
        Console.WriteLine($"Валюта     : {options.Orders.Currency}");
        Console.WriteLine($"Файл даних : {dataFile}");

        try
        {
            if (args.Contains("export"))
                Export(dataFile);
            else if (args.Contains("import"))
                Import(dataFile, options.Orders.Currency);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Файл даних пошкоджено, це не коректний JSON: {ex.Message}");
            return 1;
        }
        catch (Exception ex) when (ex is ArgumentException or DomainException)
        {
            Console.WriteLine($"Дані у файлі некоректні: {ex.Message}");
            return 1;
        }

        return 0;
    }

    // Експорт: файл даних отримує всю колекцію замовлень (старий вміст замінюється).
    private static void Export(string dataFile)
    {
        var store = new FileOrderStore(dataFile);

        foreach (var old in store.List())
            store.Remove(old.Id);

        foreach (var order in SampleData.Orders())
            store.Add(order);

        Console.WriteLine($"Експортовано {store.List().Count} записів.");
    }

    // Імпорт: читає колекцію з файлу і показує перші замовлення.
    private static void Import(string dataFile, string currency)
    {
        if (!File.Exists(dataFile))
        {
            Console.WriteLine("Файлу даних ще немає, спершу виконайте export.");
            return;
        }

        var orders = new FileOrderStore(dataFile).List();
        Console.WriteLine($"Імпортовано {orders.Count} записів.");

        foreach (var o in orders.Take(3))
            Console.WriteLine($"  {o.Id,-6} {o.CustomerId,-6} {o.Status,-10} рядків: {o.Lines.Count}  сума: {o.Total:F2} {currency}");
    }
}