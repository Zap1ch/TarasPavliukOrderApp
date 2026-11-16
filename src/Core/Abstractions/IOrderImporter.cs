using Core.Domain;

namespace Core.Abstractions;

// Асинхронне завантаження замовлень із джерела (файл, а згодом мережа чи БД).
public interface IOrderImporter
{
    // Увесь список одразу.
    Task<IReadOnlyList<Order>> LoadAsync(string path, CancellationToken ct = default);

    // Додаткове 1: потоково, по одному замовленню (await foreach).
    IAsyncEnumerable<Order> StreamAsync(string path, CancellationToken ct = default);
}