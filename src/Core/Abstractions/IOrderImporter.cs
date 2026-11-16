using Core.Domain;

namespace Core.Abstractions;

// Асинхронне завантаження замовлень із джерела (файл, а згодом мережа чи БД).
public interface IOrderImporter
{
    // Увесь список одразу.
    Task<IReadOnlyList<Order>> LoadAsync(string path, CancellationToken ct = default);

    // Додаткове 1: потоково, по одному замовленню (await foreach).
    // Додаткове 3: progress — відсоток прочитаного файлу (необов'язково).
    IAsyncEnumerable<Order> StreamAsync(string path, CancellationToken ct = default, IProgress<int>? progress = null);
}