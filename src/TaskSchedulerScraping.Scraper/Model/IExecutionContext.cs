using TaskSchedulerScraping.Scraper.Results.Context;

namespace TaskSchedulerScraping.Scraper.Model;

/// <summary>
/// Executing context should be used by a unique thread.
/// </summary>
public interface IExecutionContext<TData> : IDisposable
    where TData : class
{
    int Id { get; }
    ContextRun Context { get; }
    void Execute(TData data);
}