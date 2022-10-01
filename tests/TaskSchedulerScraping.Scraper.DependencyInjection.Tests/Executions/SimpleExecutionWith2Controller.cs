namespace TaskSchedulerScraping.Scraper.DependencyInjection.Tests.Executions;

internal class SimpleExecutionWith2Controller : SimpleExecution
{
    public readonly ISimpleService Service;
    public SimpleExecutionWith2Controller(ISimpleService service)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(Service));
        Service = service;
    }

    public SimpleExecutionWith2Controller(ISimpleService service, object? obj = null)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(Service));
        Service = service;
    }
}