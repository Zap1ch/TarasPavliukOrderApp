namespace Core.Dto;

public record OrderLineDto(
    string ProductId,
    string ProductName,
    decimal Price,
    int Quantity);
