using Core.Domain;

namespace Core.Reports;

// Усі звіти лаби 7. Приймають будь-яку колекцію замовлень і повертають готові DTO.
public static class ReportService
{
    // 1) Фільтр + сортування + проєкція: замовлення дорожчі за threshold.
    public static IReadOnlyList<OrderReportLine> ExpensiveOrders(IEnumerable<Order> source, decimal threshold) =>
        source.Where(o => o.Total > threshold)
              .OrderByDescending(o => o.Total).ThenBy(o => o.Id, StringComparer.Ordinal)
              .Select(o => new OrderReportLine(o.Id, o.CustomerId, o.Status.ToString(), o.Lines.Count, o.Total))
              .ToList();

    // 2) Топ-N клієнтів за сумою замовлень. ThenBy — другий ключ, щоб порядок не стрибав.
    public static IReadOnlyList<GroupSummary> TopCustomers(IEnumerable<Order> source, int n) =>
        source.GroupBy(o => o.CustomerId, StringComparer.OrdinalIgnoreCase)
              .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(o => o.Total)))
              .OrderByDescending(s => s.TotalSum).ThenBy(s => s.Key, StringComparer.Ordinal)
              .Take(n)
              .ToList();

    // 3) Групування за статусом: скільки замовлень і на яку суму в кожному статусі.
    public static IReadOnlyList<GroupSummary> ByStatus(IEnumerable<Order> source) =>
        source.GroupBy(o => o.Status)
              .Select(g => new GroupSummary(g.Key.ToString(), g.Count(), g.Sum(o => o.Total)))
              .OrderByDescending(s => s.Count).ThenBy(s => s.Key, StringComparer.Ordinal)
              .ToList();

    // 4) Агрегати: кількість, загальна сума, середній чек, максимальне замовлення.
    public static (int Orders, decimal Total, decimal Average, decimal Max) Totals(IEnumerable<Order> source)
    {
        var list = source.ToList();   // перебираємо джерело один раз

        return list.Count == 0
            ? (0, 0m, 0m, 0m)         // порожньо: Average і Max кинули б виняток
            : (list.Count, list.Sum(o => o.Total), list.Average(o => o.Total), list.Max(o => o.Total));
    }
}