namespace CarroCompraService.Repositories.Common;

/// <summary>
/// Interfaz genérica para operaciones CRUD básicas.
/// </summary>
public interface ICrudRepository<TKey, TEntity> where TEntity : class
{
    /// <summary>Obtiene todas las entidades.</summary>
    IEnumerable<TEntity> GetAll();
    
    /// <summary>Obtiene una entidad por su ID.</summary>
    TEntity? GetById(TKey id);
    
    /// <summary>Crea una nueva entidad.</summary>
    TEntity? Create(TEntity entity);
    
    /// <summary>Actualiza una entidad existente.</summary>
    TEntity? Update(TKey id, TEntity entity);
    
    /// <summary>Elimina una entidad (borrado lógico si tiene Deleted).</summary>
    TEntity? Delete(TKey id, bool logical = true);
}
