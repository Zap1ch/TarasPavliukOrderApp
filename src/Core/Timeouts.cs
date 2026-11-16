namespace Core;

// Додаткове 2: тайм-аут поверх токена користувача через зв'язаний токен.
public static class Timeouts
{
    // Виконує операцію, яку можна скасувати і користувачем, і тайм-аутом.
    // Якщо спрацював саме тайм-аут — кидає TimeoutException, якщо користувач — OperationCanceledException.
    public static async Task<int> RunAsync(Func<CancellationToken, Task<int>> operation,
                                           TimeSpan timeout, CancellationToken userToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        using var timeoutCts = new CancellationTokenSource(timeout);

        // Зв'язаний токен: скасується, щойно скасується БУДЬ-ЯКИЙ з двох.
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(userToken, timeoutCts.Token);

        try
        {
            return await operation(linked.Token);
        }
        catch (OperationCanceledException ex)
            when (timeoutCts.IsCancellationRequested && !userToken.IsCancellationRequested)
        {
            // Причина — таймер, а не користувач. Початковий виняток зберігаємо як inner.
            throw new TimeoutException($"Операція не завершилася за {timeout.TotalMilliseconds} мс.", ex);
        }
    }
}