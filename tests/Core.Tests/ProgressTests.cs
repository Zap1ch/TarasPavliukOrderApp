using System.Text.Json;
using Core.Dto;
using Core.Services;
using Core.Storage;

namespace Core.Tests;

public class ProgressTests
{
    // Тестовий підписник: просто запам'ятовує всі відсотки (синхронно).
    private sealed class ListProgress : IProgress<int>
    {
        public List<int> Values { get; } = new();
        public void Report(int value) => Values.Add(value);
    }

    [Fact]   // додаткове 3: відсотки лише зростають і закінчуються на 100
    public async Task ImportStreamAsync_ReportsGrowingPercentUpTo100()
    {
        string path = Path.Combine(Path.GetTempPath(), $"orders-{Guid.NewGuid():N}.json");
        try
        {
            // 2000 замовлень — файл достатньо великий, щоб читатися кількома шматками.
            var dtos = Enumerable.Range(1, 2000)
                .Select(i => new OrderDto($"T-{i}", "C-1", "Draft",
                    new List<OrderLineDto> { new("P-1", "Миша", 450m, 1) }))
                .ToList();
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(dtos));

            var progress = new ListProgress();
            var service = new OrderService(new InMemoryOrderStore());

            int added = await service.ImportStreamAsync(new JsonOrderImporter(), path, progress: progress);

            Assert.Equal(2000, added);
            Assert.True(progress.Values.Count > 1);                    // було кілька проміжних звітів
            Assert.Equal(100, progress.Values[^1]);                    // закінчилось на 100%
            Assert.Equal(progress.Values.OrderBy(v => v), progress.Values);   // лише зростали
        }
        finally
        {
            File.Delete(path);
        }
    }
}