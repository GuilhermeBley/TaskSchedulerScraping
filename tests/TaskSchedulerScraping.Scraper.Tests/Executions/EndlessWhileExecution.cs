using TaskSchedulerScraping.Scraper.Model;
using TaskSchedulerScraping.Scraper.Results.Context;

namespace TaskSchedulerScraping.Scraper.Tests.Executions;

internal class EndlessWhileExecution : ExecutionContext<SimpleData>
{
    public bool InRepeat = true;
    public Action<bool>? OnRepeat;

    public override void Dispose()
    {

    }

    public override ExecutionResult Execute(SimpleData data)
    {
        while (InRepeat)
            Thread.Sleep(50);

        return ExecutionResult.Ok();
    }
}