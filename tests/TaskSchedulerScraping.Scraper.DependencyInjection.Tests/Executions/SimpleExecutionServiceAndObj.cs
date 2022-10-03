using TaskSchedulerScraping.Scraper.DependencyInjection.Attributes;

namespace TaskSchedulerScraping.Scraper.DependencyInjection.Tests.Executions;

internal class SimpleExecutionServiceAndObj : SimpleExecution
{
    public readonly ISimpleService Service;
    public SimpleExecutionServiceAndObj(
        ISimpleService service,
        Obj1 obj)
    {
        if (service == null || obj == null)
            throw new ArgumentNullException(nameof(Service));
        Service = service;
    }
}