namespace CarroCompraService.Cache;

// Interfaz genérica para caché
public interface ICache<TKey, TValue> where TKey : notnull
{
    // Obtiene un valor por su clave
    TValue? Get(TKey key);
    // Añade o actualiza un valor
    void Put(TKey key, TValue value);
    // Elimina un valor
    void Remove(TKey key);
    // Limpia la caché
    void Clear();
    // Verifica si existe una clave
    bool Contains(TKey key);
}
