namespace Api.Contracts;

// Вхідний DTO для POST і PUT: клієнт передає лише покупця і рядки, номер і статус задає сервер.
// Правила перевірки винесені в SaveOrderRequestValidator (FluentValidation).
public sealed record SaveOrderLineRequest(string ProductId, string ProductName, decimal Price, int Quantity);

public sealed record SaveOrderRequest(string Customer, List<SaveOrderLineRequest> Lines);