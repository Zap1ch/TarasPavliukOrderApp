using Core.Domain;

namespace Core.Abstractions;

// Що вміє сховище замовлень. Де саме зберігати (пам'ять чи файл), вирішує клас, що його реалізує.
public interface IOrderStore
{
    // Лише для читання: змінювати тільки через Add / Update / Remove.
    IReadOnlyList<Order> List();

    // null, якщо замовлення немає.
    Order? GetById(string id);

    void Add(Order item);
    void Update(Order item);

    // false, якщо такого id не було.
    bool Remove(string id);
}