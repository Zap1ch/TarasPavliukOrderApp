namespace Core.Domain;

// Додаткове 3: стан замовлення.
public enum OrderStatus
{
    Draft,      // чернетка: можна додавати рядки
    Confirmed,  // підтверджене
    Cancelled   // скасоване: кінцевий стан
}