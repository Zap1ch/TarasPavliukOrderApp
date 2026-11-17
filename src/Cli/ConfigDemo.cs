using Core.Configuration;

// Лаба 9: показ налаштувань (далі тут будуть export і import).
public static class ConfigDemo
{
    public static int Run(string[] args)
    {
        var config = AppConfig.Build(args);
        var options = AppConfig.Options(config);

        Console.WriteLine($"Середовище : {options.EnvironmentName}");
        Console.WriteLine($"Рівень логу: {options.Logging.MinimumLevel}");
        Console.WriteLine($"Валюта     : {options.Orders.Currency}");
        Console.WriteLine($"Файл даних : {AppConfig.DataFile(options)}");
        return 0;
    }
}