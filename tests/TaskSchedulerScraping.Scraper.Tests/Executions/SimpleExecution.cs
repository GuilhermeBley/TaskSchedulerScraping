using TaskSchedulerScraping.Scraper.Model;
using TaskSchedulerScraping.Scraper.Results.Context;

namespace TaskSchedulerScraping.Scraper.Tests.Executions;

internal class SimpleExecution : ExecutionContext<SimpleData>
{
    private List<DateTime> _execHours { get; } = new();
    public IEnumerable<DateTime> ExecHours => _execHours;
    public ContextRun Context { get; } = new ContextRun();

    public Action<DateTime>? OnSearch;

    public override void Dispose()
    {
        
    }

    public override ExecutionResult Execute(SimpleData data)
    {
        var time = DateTime.Now;
        _execHours.Add(time);
        OnSearch?.Invoke(time);

        return ExecutionResult.Ok();
    }
}