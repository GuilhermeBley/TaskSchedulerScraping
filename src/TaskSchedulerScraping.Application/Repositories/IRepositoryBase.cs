namespace TaskSchedulerScraping.Application.Repositories;

/// <summary>
/// Persistency data base
/// </summary>
/// <typeparam name="TEntity">Entity</typeparam>
/// <typeparam name="TId">Identifier of entity</typeparam>
public interface IRepositoryBase<TEntity, TId>
{
    /// <summary>
    /// Get all entities
    /// </summary>
    /// <returns>enumerable of entity</returns>
    public Task<IEnumerable<TEntity>> GetAllAsync();

    /// <summary>
    /// Get entity by id
    /// </summary>
    /// <param name="id">identifier</param>
    /// <returns>return a entity or null</returns>
    public Task<TEntity> GetByIdOrDefaultAsync(TId id);

    /// <summary>
    /// Insert data
    /// </summary>
    /// <param name="entity">entity to add</param>
    /// <returns>return entity inserted</returns>
    public Task<TEntity> AddAsync(TEntity entity);

    /// <summary>
    /// Update data
    /// </summary>
    /// <param name="entity">entity to update</param>
    /// <returns>return entity updated</returns>
    public Task<TEntity> UpdateAsync(TEntity entity);

    /// <summary>
    /// Delete data
    /// </summary>
    /// <param name="entity">entity to delete</param>
    /// <returns>return entity deleted</returns>
    public Task<TEntity> DeleteByIdAsync(TId id);
}