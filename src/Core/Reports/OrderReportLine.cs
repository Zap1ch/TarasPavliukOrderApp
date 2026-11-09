namespace Core.Reports;

// Рядок звіту про одне замовлення. Статус — рядок, а не enum: звіт не залежить від доменної моделі.
public sealed record OrderReportLine(string OrderId, string CustomerId, string Status, int LinesCount, decimal Total);