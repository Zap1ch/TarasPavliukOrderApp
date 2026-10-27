using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

// Декоратор: обгортає будь-яке IOrderStore і кешує список замовлень.
public sealed class CachingOrderStore(IOrderStore inner) : IOrderStore
{
    private readonly IOrderStore _inner = inner ?? throw new ArgumentNullException(nameof(inner));

    // null — кеш порожній або застарів.
    private List<Order>? _cache;

    public int Hits { get; private set; }    // відповіді з кешу
    public int Misses { get; private set; }  // звернення до внутрішнього сховища

    public IReadOnlyList<Order> List()
    {
        if (_cache is null)
        {
            Misses++;
            _cache = _inner.List().ToList();
        }
        else
        {
            Hits++;
        }

        return _cache.AsReadOnly();
    }

    public Order? GetById(string id) => _inner.GetById(id);

    // Після будь-якої зміни кеш скидаємо, щоб не віддати старі дані.
    public void Add(Order item)
    {
        _inner.Add(item);
        _cache = null;
    }

    public void Update(Order item)
    {
        _inner.Update(item);
        _cache = null;
    }

    public bool Remove(string id)
    {
        bool removed = _inner.Remove(id);
        _cache = null;
        return removed;
    }
}