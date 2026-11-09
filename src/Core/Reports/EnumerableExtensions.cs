namespace Core.Reports;

// Додаткове 2: власний метод-розширення для LINQ.
public static class EnumerableExtensions
{
    // Топ-N за спаданням ключа. При однаковому ключі — за другим ключем, щоб порядок був стабільний. this перед першим параметром дозволяє викликати метод через крапку: список.TopBy(...).
    public static IReadOnlyList<T> TopBy<T>(
        this IEnumerable<T> source, Func<T, decimal> key, Func<T, string> thenKey, int n) =>
        source.OrderByDescending(key)
              .ThenBy(thenKey, StringComparer.Ordinal)
              .Take(n)
              .ToList();
}