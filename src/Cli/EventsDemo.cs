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

        // Крок 5. Сценарій: створити замовлення, додати 2 рядки.
        var created = service.CreateOrder("C-108");
        service.AddLine(created.Id, "P-5", "Кабель HDMI", 250m, 2);
        service.AddLine(created.Id, "P-3", "Монітор", 5000m, 1);

        // Крок 6. Відмова: виняток є, події немає (OnOrderChanged ще підписаний).
        Console.WriteLine();
        Console.WriteLine($"Подій до відмов: {changes}");

        try
        {
            // Від'ємна кількість — інваріант OrderLine.
            service.AddLine(created.Id, "P-1", "Клавіатура", 1200m, -1);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"  Помилка: {ex.Message}");
        }

        try
        {
            // Неіснуюче замовлення.
            service.Confirm("O-999");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"  Помилка: {ex.Message}");
        }

        Console.WriteLine($"Подій після відмов: {changes}");
        Console.WriteLine();

        // Продовження сценарію: підтвердження.
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

        // Крок 7. Func замість події: тут потрібен результат (список), а не сповіщення.
        var small = service.Search(o => o.Total < 1000m);

        Console.WriteLine();
        Console.WriteLine($"Замовлення із сумою менше 1000 ({small.Count}):");
        foreach (var o in small)
            Console.WriteLine($"  {o.Id,-8} {o.CustomerId,-6} {o.Status,-10} сума: {o.Total,9:F2}");
    }
}