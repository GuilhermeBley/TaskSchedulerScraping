using TaskSchedulerScraping.Scraper.Results;
using TaskSchedulerScraping.Scraper.Results.Models;

namespace TaskSchedulerScraping.Scraper.Model;

/// <summary>
/// Basics functions for process
/// </summary>
public interface IModelScraper
{
    /// <summary>
    /// Identifier
    /// </summary>
    Guid IdScraper { get; }

    /// <summary>
    /// Number of scraper to execute your context
    /// </summary>
    int CountScraper { get; }

    /// <summary>
    /// Get current state
    /// </summary>
    ModelStateEnum State { get; }

    /// <summary>
    /// Requests pause and wait async
    /// </summary>
    /// <remarks>
    ///     <param name="pause">True to pause, false to unpause</param>
    /// </remarks>
    /// <param name="cancellationToken">Cancel waiting if cancellation is requested</param>
    /// <returns><see cref="ResultBase{PauseModel}"/></returns>
    /// <exception cref="OperationCanceledException"/>
    Task<ResultBase<PauseModel>> PauseAsync(bool pause = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Request and run scrapers
    /// </summary>
    /// <remarks>
    ///     <para>This method is awaitable because the data to search is collected async</para>
    /// </remarks>
    /// <returns><see cref="ResultBase{RunModel}"/></returns>
    Task<ResultBase<RunModel>> Run();

    /// <summary>
    /// Requests stop and wait async
    /// </summary>
    /// <param name="cancellationToken">Cancel waiting if cancellation is requested</param>
    /// <returns><see cref="ResultBase{StopModel}"/></returns>
    /// <exception cref="OperationCanceledException"/>
    Task<ResultBase<StopModel>> StopAsync(CancellationToken cancellationToken = default);
}