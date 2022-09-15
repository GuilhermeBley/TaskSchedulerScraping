using TaskSchedulerScraping.Scraper.Results.Context;

namespace TaskSchedulerScraping.Scraper.Model;

public abstract class ExecutionContext<TData> : IDisposable
    where TData : class
{
    /// <summary>
    /// Thread which execute the class
    /// </summary>
    public int Id => Context.IdThread;

    /// <summary>
    /// Context in execution
    /// </summary>
    internal ContextRun Context { get; } = new();

    
    public void CheckState()
    {

    }

    /// <summary>
    /// Execution
    /// </summary>
    /// <param name="data">Data to execute</param>
    public abstract void Execute(TData data);

    /// <summary>
    /// Dispose resources from instance
    /// </summary>
    public virtual void Dispose()
    {

    }
}