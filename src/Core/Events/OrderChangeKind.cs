namespace Core.Events;

// Що саме сталося із замовленням.
public enum OrderChangeKind
{
    OrderCreated,   // створено нове замовлення
    LineAdded,      // додано рядок
    OrderConfirmed  // замовлення підтверджено
}