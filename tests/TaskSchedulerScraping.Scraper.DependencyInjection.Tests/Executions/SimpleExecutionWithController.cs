namespace TaskSchedulerScraping.Scraper.DependencyInjection.Tests.Executions;

internal class SimpleExecutionWithController : SimpleExecution
{
    public readonly ISimpleService Service;

    /// <summary>
    /// Instance with a service
    /// </summary>
    /// <param name="service">service</param>
    /// <exception cref="ArgumentNullException">argument null</exception>
    public SimpleExecutionWithController(ISimpleService service)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(Service));
        Service = service;
    }
}