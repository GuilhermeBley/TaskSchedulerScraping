using TaskSchedulerScraping.Scraper.Model;
using TaskSchedulerScraping.Scraper.Results.Context;

namespace TaskSchedulerScraping.Scraper.Tests.Executions;

internal class EndlessExecution : ExecutionContext<SimpleData>
{
    public ContextRun Context { get; } = new ContextRun();

    public Action? OnRepeat;

    public override void Dispose()
    {
        
    }

    public override ExecutionResult Execute(SimpleData data)
    {
        while (true)
        {
            Thread.Sleep(50);
            OnRepeat?.Invoke();

            CheckState();
        }
    }
}