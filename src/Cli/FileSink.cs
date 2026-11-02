using Core.Abstractions;
using Core.Events;

// Одержувач: дописує сповіщення у файл.
public sealed class FileSink(string path) : INotificationSink
{
    public void Notify(OrderChangedEventArgs e)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.AppendAllText(path,
            $"{e.At:HH:mm:ss} {e.Kind,-14} {e.OrderId} рядків: {e.LinesCount} → сума {e.Total:F2}" + Environment.NewLine);
    }
}