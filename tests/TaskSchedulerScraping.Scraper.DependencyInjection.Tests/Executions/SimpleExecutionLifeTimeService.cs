namespace TaskSchedulerScraping.Scraper.DependencyInjection.Tests.Executions;

/// <summary>
/// Test life time services with this class
/// </summary>
internal class SimpleExecutionLifeTimeService : SimpleExecution
{
    public readonly ISimpleService Service;
    public readonly IServiceLinkedWithOther LinkedService;
    public SimpleExecutionLifeTimeService(
        ISimpleService service,
        IServiceLinkedWithOther linkedService)
    {
        if (service == null || linkedService == null)
            throw new ArgumentNullException(nameof(Service));
        Service = service;
        LinkedService = linkedService;
    }
}