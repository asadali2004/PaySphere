using PaySphere.BuildingBlocks.Base;

namespace PaySphere.AuthService.Repositories.Interfaces;

/// <summary>
/// Generic repository abstraction for EF Core entities. Repositories perform
/// database access and do not contain business rules.
/// </summary>
/// <typeparam name="T">Entity type inheriting from <see cref="BaseEntity"/>.</typeparam>
public interface IGenericRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Retrieves an entity by its primary key.
    /// </summary>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Returns all entities of the given type.
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Adds a new entity to the underlying set.
    /// </summary>
    Task AddAsync(T entity);

    /// <summary>
    /// Marks the entity as modified.
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Removes the entity from the underlying set.
    /// </summary>
    void Delete(T entity);

    /// <summary>
    /// Persists pending changes to the database.
    /// </summary>
    Task SaveChangesAsync();
}
