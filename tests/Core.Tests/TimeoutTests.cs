namespace Core.Tests;

public class TimeoutTests
{
    // "Нескінченна" операція: чекає, поки її не скасують.
    private static async Task<int> WaitForever(CancellationToken ct)
    {
        await Task.Delay(Timeout.Infinite, ct);
        return 0;
    }

    [Fact]   // додаткове 2: доводимо, що спрацював саме тайм-аут
    public async Task RunAsync_WhenTimeoutExpires_ThrowsTimeoutException()
    {
        using var user = new CancellationTokenSource();   // користувач нічого не скасовує

        var ex = await Assert.ThrowsAsync<TimeoutException>(
            () => Timeouts.RunAsync(WaitForever, TimeSpan.FromMilliseconds(50), user.Token));

        Assert.IsAssignableFrom<OperationCanceledException>(ex.InnerException);   // причина збережена
        Assert.False(user.IsCancellationRequested);                               // користувач не натискав
    }

    [Fact]   // скасування користувачем — НЕ тайм-аут
    public async Task RunAsync_WhenUserCancels_ThrowsOperationCanceled()
    {
        using var user = new CancellationTokenSource();
        user.CancelAfter(50);   // "Ctrl+C" через 50 мс, тайм-аут аж 10 с

        // TimeoutException не є OperationCanceledException, тож при тайм-ауті тест упав би
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => Timeouts.RunAsync(WaitForever, TimeSpan.FromSeconds(10), user.Token));
    }

    [Fact]   // швидка операція встигає — результат повертається як є
    public async Task RunAsync_FastOperation_ReturnsResult()
    {
        int result = await Timeouts.RunAsync(_ => Task.FromResult(42), TimeSpan.FromSeconds(1));

        Assert.Equal(42, result);
    }
}