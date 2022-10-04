using TaskSchedulerScraping.Scraper.DependencyInjection.Attributes;

namespace TaskSchedulerScraping.Scraper.DependencyInjection.Tests.Executions;

/// <summary>
/// Class with shared service
/// </summary>
internal class SimpleExecutionShareService : SimpleExecution
{
    public ISimpleService SharedService { get; }

    /// <summary>
    /// Instance with simple service
    /// </summary>
    /// <param name="serviceShared">Simple service with attribute of shared service</param>
    public SimpleExecutionShareService([SharedService] ISimpleService serviceShared)
    {
        SharedService = serviceShared;
    }
}