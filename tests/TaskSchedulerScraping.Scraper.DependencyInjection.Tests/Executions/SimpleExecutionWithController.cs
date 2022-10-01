namespace TaskSchedulerScraping.Scraper.DependencyInjection.Tests.Executions;

internal class SimpleExecutionWithController : SimpleExecution
{
    public readonly ISimpleService Service;
    public SimpleExecutionWithController(ISimpleService service)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(Service));
        Service = service;
    }
}