namespace TaskSchedulerScraping.Scraper.DependencyInjection.Tests.Executions;

internal class SimpleExecutionServiceAndObj : SimpleExecution
{
    public readonly ISimpleService Service;
    public SimpleExecutionServiceAndObj(
        ISimpleService service)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(Service));
        Service = service;
    }
}