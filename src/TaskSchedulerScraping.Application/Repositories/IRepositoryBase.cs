namespace TaskSchedulerScraping.Application.Repositories;

/// <summary>
/// Persistency data base
/// </summary>
/// <typeparam name="TEntity">Entity</typeparam>
/// <typeparam name="TId">Identifier of entity</typeparam>
public interface IRepositoryBase<TEntity, TEntityDto, TId>
{
    /// <summary>
    /// Get all entities
    /// </summary>
    /// <returns>enumerable of entity</returns>
    public Task<IEnumerable<TEntityDto>> GetAllAsync();

    /// <summary>
    /// Get entity by id
    /// </summary>
    /// <param name="id">identifier</param>
    /// <returns>return a entity or null</returns>
    public Task<TEntityDto?> GetByIdOrDefaultAsync(TId id);

    /// <summary>
    /// Insert data
    /// </summary>
    /// <param name="entity">entity to add</param>
    /// <returns>return entity inserted or null if failed to add</returns>
    public Task<TEntityDto?> AddAsync(TEntity entity);

    /// <summary>
    /// Update data
    /// </summary>
    /// <param name="entity">entity to update</param>
    /// <returns>return entity inserted or null if failed to add</returns>
    public Task<TEntityDto?> UpdateAsync(TEntity entity);

    /// <summary>
    /// Delete data
    /// </summary>
    /// <param name="entity">entity to delete</param>
    /// <returns>return entity inserted or null if failed to add</returns>
    public Task<TEntityDto?> DeleteByIdAsync(TId id);
}