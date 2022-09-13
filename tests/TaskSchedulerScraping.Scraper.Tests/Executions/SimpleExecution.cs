using TaskSchedulerScraping.Scraper.Model;
using TaskSchedulerScraping.Scraper.Results.Context;

namespace TaskSchedulerScraping.Scraper.Tests.Executions;

internal class SimpleExecution : IExecutionContext<SimpleData>
{
    private List<DateTime> _execHours { get; } = new();
    public IEnumerable<DateTime> ExecHours => _execHours;
    public int Id => 0;

    public ContextRun Context { get; } = new ContextRun();

    public void Dispose()
    {
        
    }

    public void Execute(SimpleData data)
    {
        Thread.Sleep(100);
        _execHours.Add(DateTime.Now);
        Thread.Sleep(100);
    }
}