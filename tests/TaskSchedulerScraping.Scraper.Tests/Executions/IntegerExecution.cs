using TaskSchedulerScraping.Scraper.Model;
using TaskSchedulerScraping.Scraper.Results.Context;

namespace TaskSchedulerScraping.Scraper.Tests.Executions;

internal class IntegerExecution : ExecutionContext<IntegerData>
{
    public ContextRun Context { get; } = new ContextRun();

    public Action<IntegerData>? OnSearch;

    public override void Dispose()
    {
        
    }

    public override ExecutionResult Execute(IntegerData data, CancellationToken cancellationToken = default)
    {
        OnSearch?.Invoke(data);

        return ExecutionResult.Ok();
    }
}