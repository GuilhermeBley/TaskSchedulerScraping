using TaskSchedulerScraping.Scraper.Results;
using TaskSchedulerScraping.Scraper.Results.Models;

namespace TaskSchedulerScraping.Scraper.Model;

/// <summary>
/// Basics functions for process
/// </summary>
public interface IModelScraper
{
    /// <summary>
    /// Requests pause async
    /// </summary>
    /// <remarks>
    ///     <param name="pause">True to pause, false to unpause</param>
    /// </remarks>
    /// <returns><see cref="ResultBase{PauseModel}"/></returns>
    Task<ResultBase<PauseModel>> PauseAsync(bool pause = true);

    /// <summary>
    /// Requests run async
    /// </summary>
    /// <returns><see cref="ResultBase{RunModel}"/></returns>
    Task<ResultBase<RunModel>> RunAsync();

    /// <summary>
    /// Requests stop async
    /// </summary>
    /// <returns><see cref="ResultBase{StopModel}"/></returns>
    Task<ResultBase<StopModel>> StopAsync();
}