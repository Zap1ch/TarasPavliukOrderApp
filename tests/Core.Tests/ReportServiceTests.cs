using Core.Reports;

namespace Core.Tests;

public class ReportServiceTests
{
    [Theory]   // 3) звіт на кількох наборах
    [InlineData(100, 5)]    // усі дорожчі за 100
    [InlineData(900, 3)]    // 1000, 3000, 3000
    [InlineData(5000, 0)]   // жодного
    public void ExpensiveOrders_CountsOnlyAboveThreshold(int minTotal, int expected) =>
        Assert.Equal(expected, ReportService.ExpensiveOrders(TestData.Orders(), minTotal).Count);

    [Fact]   // 4) детермінований топ-N з лаби 7: при нічиї C-2 раніше за C-3 (ThenBy)
    public void TopCustomers_IsDeterministic() =>
        Assert.Equal(new[] { "C-2", "C-3" },
                     ReportService.TopCustomers(TestData.Orders(), 2).Select(c => c.Key));
}