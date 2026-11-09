namespace Core.Reports;

// Зведений рядок: клієнт з іменем + кількість і сума його замовлень (результат GroupJoin).
public sealed record CustomerReport(string CustomerId, string Name, int OrdersCount, decimal TotalSum);