namespace Api.Contracts;

// Відповідь для списку сторінками: елементи поточної сторінки + скільки всього знайдено
public record PagedResponse<T>(IReadOnlyList<T> Items, int Total);