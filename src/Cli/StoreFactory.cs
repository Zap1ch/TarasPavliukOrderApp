using Core;
using Core.Abstractions;
using Core.Storage;

// Фабрика сховищ: вибирає реалізацію за аргументами командного рядка.
// Лежить у Cli, бо створює конкретні класи (частина composition root).
public static class StoreFactory
{
    public static string DataPath => Path.Combine(AppContext.BaseDirectory, "data", "orders.json");

    public static IOrderStore Create(string[] args)
    {
        if (args.Contains("--file"))
            return new FileOrderStore(DataPath);

        return new InMemoryOrderStore(SampleData.Orders());
    }
}