namespace Core;

// Порушення бізнес-правила: аргументи коректні, але операція заборонена станом замовлення.
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    // inner — початкова помилка, яку загортаємо (знадобиться в імпорті).
    public DomainException(string message, Exception inner) : base(message, inner) { }
}