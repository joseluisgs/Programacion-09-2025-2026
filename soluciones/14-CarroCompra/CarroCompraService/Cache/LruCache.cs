using System.Collections.Concurrent;

namespace CarroCompraService.Cache;

// Implementación de LRU Cache (Least Recently Used)
// Elimina automáticamente los elementos menos usados cuando se llena
public class LruCache<TKey, TValue> : ICache<TKey, TValue> where TKey : notnull
{
    // Capacidad máxima de la caché
    private readonly int _capacity;
    // Diccionario para almacenar los valores
    private readonly ConcurrentDictionary<TKey, TValue> _cache = new();
    // Orden de uso (los últimos usados están al final)
    private readonly LinkedList<TKey> _order = new();
    // Diccionario para buscar en la lista enlazada
    private readonly ConcurrentDictionary<TKey, LinkedListNode<TKey>> _nodes = new();
    // Lock para operacionesthread-safe
    private readonly object _lock = new();

    // Constructor con capacidad
    public LruCache(int capacity)
    {
        _capacity = capacity > 0 ? capacity : 3;
    }

    // Obtiene un valor y lo marca como usado
    public TValue? Get(TKey key)
    {
        if (_cache.TryGetValue(key, out var value))
        {
            // Mover al final (más recientemente usado)
            MoveToEnd(key);
            return value;
        }
        return default;
    }

    // Añade o actualiza un valor
    public void Put(TKey key, TValue value)
    {
        lock (_lock)
        {
            // Si ya existe, actualizar y mover al final
            if (_cache.ContainsKey(key))
            {
                _cache[key] = value;
                MoveToEnd(key);
                return;
            }

            // Si está llena, eliminar el menos usado
            if (_cache.Count >= _capacity)
            {
                RemoveLeastRecentlyUsed();
            }

            // Añadir al final
            var node = _order.AddLast(key);
            _nodes[key] = node;
            _cache[key] = value;
        }
    }

    // Elimina un valor
    public void Remove(TKey key)
    {
        lock (_lock)
        {
            if (_nodes.TryRemove(key, out var node))
            {
                _order.Remove(node);
                _cache.TryRemove(key, out _);
            }
        }
    }

    // Limpia la caché
    public void Clear()
    {
        lock (_lock)
        {
            _cache.Clear();
            _order.Clear();
            _nodes.Clear();
        }
    }

    // Verifica si existe una clave
    public bool Contains(TKey key)
    {
        return _cache.ContainsKey(key);
    }

    // Mueve un elemento al final de la lista
    private void MoveToEnd(TKey key)
    {
        if (_nodes.TryGetValue(key, out var node) && node != _order.Last)
        {
            _order.Remove(node);
            var newNode = _order.AddLast(key);
            _nodes[key] = newNode;
        }
    }

    // Elimina el elemento menos usado (el primero)
    private void RemoveLeastRecentlyUsed()
    {
        if (_order.First != null)
        {
            var firstKey = _order.First.Value;
            _order.RemoveFirst();
            _nodes.TryRemove(firstKey, out _);
            _cache.TryRemove(firstKey, out _);
        }
    }
}
