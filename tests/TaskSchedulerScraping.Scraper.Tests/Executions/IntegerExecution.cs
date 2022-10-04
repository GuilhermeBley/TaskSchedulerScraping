using TaskSchedulerScraping.Scraper.Model;
using TaskSchedulerScraping.Scraper.Results.Context;

namespace TaskSchedulerScraping.Scraper.Tests.Executions;

internal class IntegerExecution : Quest<IntegerData>
{
    public ContextRun Context { get; } = new ContextRun();

    public Action<IntegerData>? OnSearch;

    public override void Dispose()
    {
        
    }

    public override QuestResult Execute(IntegerData data, CancellationToken cancellationToken = default)
    {
        OnSearch?.Invoke(data);

        return QuestResult.Ok();
    }
}