using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

// Читає замовлення з JSON асинхронно, з підтримкою скасування.
public sealed class JsonOrderImporter : IOrderImporter
{
    public async Task<IReadOnlyList<Order>> LoadAsync(string path, CancellationToken ct = default)
    {
        // Якщо вже скасовано — навіть не відкриваємо файл.
        ct.ThrowIfCancellationRequested();

        // useAsync: true — справжнє асинхронне читання, без Task.Run.
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                                                FileShare.Read, 4096, useAsync: true);

        var dtos = await JsonSerializer.DeserializeAsync<List<OrderDto>>(stream, cancellationToken: ct)
                   ?? throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку замовлень.");

        var result = new List<Order>(dtos.Count);
        for (var i = 0; i < dtos.Count; i++)
        {
            ct.ThrowIfCancellationRequested();   // перевірка на кожному записі

            try
            {
                // Ті самі перевірки, що й у лабі 4.
                result.Add(Order.FromDto(dtos[i]));
            }
            catch (ArgumentException ex)
            {
                // Поганий аргумент у записі — для імпорту це відхилені дані.
                throw new DomainException($"Запис {i + 1}: {ex.Message}", ex);
            }
            catch (DomainException ex)
            {
                // Порушене правило замовлення — додаємо номер запису.
                throw new DomainException($"Запис {i + 1}: {ex.Message}", ex);
            }
        }

        return result;
    }
}