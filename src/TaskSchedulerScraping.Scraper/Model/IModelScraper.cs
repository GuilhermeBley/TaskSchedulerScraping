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
    /// Unique name
    /// </summary>
    string ModelScraperName { get; }

    /// <summary>
    /// Requests pause async
    /// </summary>
    /// <remarks>
    ///     <param name="pause">True to pause, false to unpause</param>
    /// </remarks>
    /// <returns><see cref="ResultBase{PauseModel}"/></returns>
    Task<ResultBase<PauseModel>> PauseAsync(bool pause = true);

    /// <summary>
    /// Requests run scrapers
    /// </summary>
    /// <returns><see cref="ResultBase{RunModel}"/></returns>
    ResultBase<RunModel> RunAsync();

    /// <summary>
    /// Requests stop async
    /// </summary>
    /// <returns><see cref="ResultBase{StopModel}"/></returns>
    Task<ResultBase<StopModel>> StopAsync();
}