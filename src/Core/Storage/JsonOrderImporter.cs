using System.Runtime.CompilerServices;
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
            result.Add(ToOrder(dtos[i], i + 1));
        }

        return result;
    }

    // Додаткове 1: потоковий імпорт. [EnumeratorCancellation] — токен з WithCancellation(ct) потрапить сюди.
    public async IAsyncEnumerable<Order> StreamAsync(string path,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                                                FileShare.Read, 4096, useAsync: true);

        // Розбирає JSON-масив по одному елементу, не читаючи весь файл у пам'ять.
        var number = 0;
        await foreach (var dto in JsonSerializer.DeserializeAsyncEnumerable<OrderDto>(stream, cancellationToken: ct))
        {
            ct.ThrowIfCancellationRequested();
            number++;
            yield return ToOrder(dto, number);   // віддаємо одразу, не чекаючи кінця файлу
        }
    }

    // DTO -> замовлення; поганий запис -> DomainException з номером запису.
    private static Order ToOrder(OrderDto? dto, int number)
    {
        try
        {
            // Ті самі перевірки, що й у лабі 4.
            return Order.FromDto(dto!);
        }
        catch (ArgumentException ex)
        {
            // Поганий аргумент у записі — для імпорту це відхилені дані.
            throw new DomainException($"Запис {number}: {ex.Message}", ex);
        }
        catch (DomainException ex)
        {
            // Порушене правило замовлення — додаємо номер запису.
            throw new DomainException($"Запис {number}: {ex.Message}", ex);
        }
    }
}