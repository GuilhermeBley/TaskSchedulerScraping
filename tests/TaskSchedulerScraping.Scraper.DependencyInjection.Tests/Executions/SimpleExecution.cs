using TaskSchedulerScraping.Scraper.Model;

namespace TaskSchedulerScraping.Scraper.DependencyInjection.Tests.Executions;

internal class SimpleExecution : ExecutionContext<SimpleData>
{
    public override void Dispose()
    {
        
    }

    public override ExecutionResult Execute(SimpleData data, CancellationToken cancellationToken = default)
    {
        Thread.Sleep(10);
        return ExecutionResult.Ok();
    }
}