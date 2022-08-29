using System.Data;

namespace TaskSchedulerScraping.Application.UoW;

/// <summary>
/// Unit of work give a shared transactions to repositorys
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Identifier of unit
    /// </summary>
    Guid Identifier { get; }

    /// <summary>
    /// Necessary to create and open a new connection
    /// </summary>
    /// <returns>async result of <see cref="IUnitOfWork"/> opened</returns>
    Task<IUnitOfWork> OpenConnectionAsync();

    /// <summary>
    /// Creates a connection (if method <see cref="OpenConnectionAsync"/> haven't executed) and transaction
    /// </summary>
    /// <remarks>
    ///     <para>Starts a transaction to the repositorys</para>
    ///     <para>Use <see cref="SaveChangesAsync"/>> after execute</para>
    /// </remarks>
    /// <returns>async result of <see cref="IUnitOfWork"/> opened</returns>
    Task<IUnitOfWork> BeginTransactionAsync();

    /// <summary>
    /// Commits if is ok or roll back if throw a exception
    /// </summary>
    /// <returns>async</returns>
    Task SaveChangesAsync();
}