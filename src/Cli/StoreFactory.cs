using Core;
using Core.Abstractions;
using Core.Storage;

// Фабрика сховищ: вибирає реалізацію за аргументами командного рядка.
// Лаба 9: шлях до файлу береться з конфігурації (appsettings / змінні середовища), а не з коду.
public static class StoreFactory
{
    public static string DataPath => AppConfig.DataFile(AppConfig.Options(AppConfig.Build([])));

    public static IOrderStore Create(string[] args)
    {
        if (args.Contains("--file"))
            return new FileOrderStore(DataPath);

        return new InMemoryOrderStore(SampleData.Orders());
    }
}