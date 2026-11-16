using Core.Domain;

namespace Core.Abstractions;

// Асинхронне завантаження замовлень із джерела (файл, а згодом мережа чи БД).
public interface IOrderImporter
{
    Task<IReadOnlyList<Order>> LoadAsync(string path, CancellationToken ct = default);
}