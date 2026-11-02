using Core.Events;
using Core.Services;

// Лаба 6: підписка на події сервісу, лог у консоль і файл.
public static class EventsDemo
{
    public static void Run(OrderService service)
    {
        // Шлях до логу будуємо від папки запуску, каталог logs створюємо кодом.
        string logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

        Console.WriteLine($"Лог: {logPath}");
        Console.WriteLine();

        // Крок 3. Підписник 1: локальний метод (можна відписати через -=).
        void OnOrderChanged(object? sender, OrderChangedEventArgs e)
        {
            string line = $"{e.At:HH:mm:ss} {e.Kind,-14} {e.OrderId} рядків: {e.LinesCount} → сума {e.Total:F2}";
            Console.WriteLine(line);
            File.AppendAllText(logPath, line + Environment.NewLine);
        }

        service.Changed += OnOrderChanged;

        // Крок 4. Підписник 2: лямбда-лічильник (замикання над changes).
        int changes = 0;
        service.Changed += (_, _) => changes++;
    }
}