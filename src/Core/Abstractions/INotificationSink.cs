using Core.Events;

namespace Core.Abstractions;

// Одержувач сповіщень про зміни замовлень. Передається в сервіс через конструктор.
public interface INotificationSink
{
    void Notify(OrderChangedEventArgs e);
}