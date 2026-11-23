namespace Core.Dto;

// CreatedAt необов'язковий: старі файли і код без дати працюють як раніше.
public record OrderDto(
    string Id,
    string CustomerId,
    string Status,
    IReadOnlyList<OrderLineDto> Lines,
    DateTime? CreatedAt = null);