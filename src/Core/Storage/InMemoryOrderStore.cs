using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

// Сховище в пам'яті: після закриття програми дані зникають.
// Лаба 10: у веб-API одне сховище (Singleton) обслуговує одночасні запити, тому кожна операція — під lock.
public sealed class InMemoryOrderStore(IEnumerable<Order>? seed = null) : IOrderStore
{
    // Ключ — номер замовлення, без урахування регістру ("o-001" == "O-001").
    private readonly Dictionary<string, Order> _items =
        (seed ?? []).ToDictionary(o => o.Id, StringComparer.OrdinalIgnoreCase);

    // Замок: поки один запит працює зі словником, інші чекають.
    private readonly object _lock = new();

    public IReadOnlyList<Order> List()
    {
        lock (_lock)
            return _items.Values.ToList();   // копія, щоб її можна було читати поза замком
    }

    public Order? GetById(string id)
    {
        lock (_lock)
            return _items.GetValueOrDefault(id);
    }

    public void Update(Order item)
    {
        lock (_lock)
            _items[item.Id] = item;
    }

    public bool Remove(string id)
    {
        lock (_lock)
            return _items.Remove(id);
    }

    public void Add(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (_lock)
        {
            // Унікальність номера перевіряє сховище, бо це правило для всього набору замовлень.
            if (_items.ContainsKey(item.Id))
                throw new InvalidOperationException($"Замовлення з id={item.Id} уже існує.");

            _items.Add(item.Id, item);
        }
    }
}