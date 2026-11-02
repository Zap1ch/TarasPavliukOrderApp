using Core.Abstractions;
using Core.Events;

// Одержувач: друкує сповіщення в консоль.
public sealed class ConsoleSink : INotificationSink
{
    public void Notify(OrderChangedEventArgs e) =>
        Console.WriteLine($"[console] {e.At:HH:mm:ss} {e.Kind,-14} {e.OrderId} рядків: {e.LinesCount} → сума {e.Total:F2}");
}