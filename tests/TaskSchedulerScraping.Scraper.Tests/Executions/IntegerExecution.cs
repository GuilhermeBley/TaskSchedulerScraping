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

    public override void Execute(IntegerData data)
    {
        OnSearch?.Invoke(data);
    }
}