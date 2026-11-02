using Core.Events;
using Core.Services;

// Лаба 6: підписка на події сервісу, лог у консоль і файл, відписка.
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

        // Крок 5. Сценарій: створити замовлення, додати 2 рядки, підтвердити.
        var created = service.CreateOrder("C-108");
        service.AddLine(created.Id, "P-5", "Кабель HDMI", 250m, 2);
        service.AddLine(created.Id, "P-3", "Монітор", 5000m, 1);
        service.Confirm(created.Id);

        Console.WriteLine();
        Console.WriteLine($"Подій отримано: {changes}");

        // Відписка: OnOrderChanged більше не слухає, лямбда-лічильник — так.
        service.Changed -= OnOrderChanged;
        Console.WriteLine("OnOrderChanged відписано.");

        // Тиха операція: замовлення створюється, але рядка в консолі й лозі немає.
        var second = service.CreateOrder("C-109");
        Console.WriteLine($"Створено замовлення {second.Id} без повідомлення в лозі");
        Console.WriteLine($"Подій отримано: {changes}");
        Console.WriteLine($"Замовлень у сховищі: {service.All().Count}");
    }
}