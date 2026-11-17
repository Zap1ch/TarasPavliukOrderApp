using System.Reflection;
using Core.Configuration;
using Microsoft.Extensions.Configuration;

// Лаба 9: збирає налаштування з усіх джерел. Кожне наступне перекриває попереднє.
public static class AppConfig
{
    public static IConfiguration Build(string[] args)
    {
        // Профіль: змінна CROSSAPP_ENVIRONMENT, без неї — Development.
        string envName = Environment.GetEnvironmentVariable("CROSSAPP_ENVIRONMENT") ?? "Development";

        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)                     // appsettings шукаємо поруч з програмою
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{envName}.json", optional: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
            .AddEnvironmentVariables(prefix: "CROSSAPP_")              // CROSSAPP_Storage__DataPath -> Storage:DataPath
            .AddCommandLine(args)
            .Build();
    }

    // Розкладає ключі конфігурації у класи AppOptions.
    public static AppOptions Options(IConfiguration config) =>
        config.Get<AppOptions>() ?? new AppOptions();

    // Повний шлях до файлу даних (від каталогу програми, а не від папки запуску).
    public static string DataFile(AppOptions options)
    {
        // Проста змінна CROSSAPP_DATA_PATH перекриває Storage:DataPath з будь-якого джерела.
        string? simplePath = Environment.GetEnvironmentVariable("CROSSAPP_DATA_PATH");

        var storage = new StorageOptions
        {
            DataPath = string.IsNullOrWhiteSpace(simplePath) ? options.Storage.DataPath : simplePath,
            FileName = options.Storage.FileName
        };

        return storage.ResolveFullPath(AppContext.BaseDirectory);
    }
}