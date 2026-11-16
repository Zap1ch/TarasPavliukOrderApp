using Core.Services;
using Core.Storage;

namespace Core.Tests;

public class ImportTests
{
    [Fact]   // 5) скасування: токен уже скасований — файл навіть не відкривається
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var service = new OrderService(new InMemoryOrderStore());

        // ThrowsAny: приймає і нащадка TaskCanceledException
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(new JsonOrderImporter(), "any.json", cts.Token));
    }

    [Fact]   // 6) тимчасовий файл з некоректним записом
    public async Task ImportAsync_FileWithBadRecord_ThrowsDomainException()
    {
        string path = Path.Combine(Path.GetTempPath(), $"orders-{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(path, """
                [
                  { "Id": "T-1", "CustomerId": "C-1", "Status": "Draft",
                    "Lines": [ { "ProductId": "P-1", "ProductName": "Миша", "Price": 450, "Quantity": 0 } ] }
                ]
                """);
            var store = new InMemoryOrderStore();
            var service = new OrderService(store);

            var ex = await Assert.ThrowsAsync<DomainException>(
                () => service.ImportAsync(new JsonOrderImporter(), path));

            Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);   // причина збережена
            Assert.Empty(store.List());                                       // нічого не додано
        }
        finally
        {
            File.Delete(path);   // прибираємо за собою навіть при падінні тесту
        }
    }

    [Fact]   // повторний імпорт нічого не дублює
    public async Task ImportAsync_Twice_IsIdempotent()
    {
        string path = Path.Combine(Path.GetTempPath(), $"orders-{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(path, """
                [
                  { "Id": "T-1", "CustomerId": "C-1", "Status": "Draft",
                    "Lines": [ { "ProductId": "P-1", "ProductName": "Миша", "Price": 450, "Quantity": 2 } ] },
                  { "Id": "T-2", "CustomerId": "C-2", "Status": "Draft",
                    "Lines": [ { "ProductId": "P-2", "ProductName": "Монітор", "Price": 5000, "Quantity": 1 } ] }
                ]
                """);
            var store = new InMemoryOrderStore();
            var service = new OrderService(store);

            int first = await service.ImportAsync(new JsonOrderImporter(), path);
            int second = await service.ImportAsync(new JsonOrderImporter(), path);

            Assert.Equal(2, first);
            Assert.Equal(0, second);
            Assert.Equal(2, store.List().Count);
        }
        finally
        {
            File.Delete(path);
        }
    }
}