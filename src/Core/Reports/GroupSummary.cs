namespace Core.Reports;

// Підсумок групи: ключ (статус або покупець), кількість замовлень і загальна сума.
public sealed record GroupSummary(string Key, int Count, decimal TotalSum);