using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

// Сховище в пам'яті: після закриття програми дані зникають.
public sealed class InMemoryOrderStore(IEnumerable<Order>? seed = null) : IOrderStore
{
    // Ключ — номер замовлення, без урахування регістру ("o-001" == "O-001").
    private readonly Dictionary<string, Order> _items =
        (seed ?? []).ToDictionary(o => o.Id, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<Order> List() => _items.Values.ToList();

    public Order? GetById(string id) => _items.GetValueOrDefault(id);

    public void Update(Order item) => _items[item.Id] = item;

    public bool Remove(string id) => _items.Remove(id);

    public void Add(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);

        // Унікальність номера перевіряє сховище, бо це правило для всього набору замовлень.
        if (_items.ContainsKey(item.Id))
            throw new InvalidOperationException($"Замовлення з id={item.Id} уже існує.");

        _items.Add(item.Id, item);
    }
}