namespace Core.Configuration;

// Налаштування застосунку. Звичайні класи: Core не знає, звідки вони прийшли.
public sealed class AppOptions
{
    public string EnvironmentName { get; init; } = "Development";
    public LoggingOptions Logging { get; init; } = new();
    public StorageOptions Storage { get; init; } = new();
    public OrdersOptions Orders { get; init; } = new();
}

public sealed class LoggingOptions
{
    public string MinimumLevel { get; init; } = "Information";
}

public sealed class OrdersOptions
{
    public string Currency { get; init; } = "UAH";
}

public sealed class StorageOptions
{
    public string DataPath { get; init; } = "data";
    public string FileName { get; init; } = "orders.json";

    // Повний шлях до файлу даних: відносний DataPath рахується від baseDir, абсолютний береться як є.
    public string ResolveFullPath(string baseDir)
    {
        var root = Path.IsPathRooted(DataPath) ? DataPath : Path.Combine(baseDir, DataPath);
        Directory.CreateDirectory(root);   // немає папки — створюємо
        return Path.GetFullPath(Path.Combine(root, FileName));
    }
}