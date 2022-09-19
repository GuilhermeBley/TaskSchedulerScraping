using TaskSchedulerScraping.Scraper.Results.Context;
using TaskSchedulerScraping.Scraper.Exceptions;

namespace TaskSchedulerScraping.Scraper.Model;

/// <summary>
/// Context execution
/// </summary>
/// <typeparam name="TData">Works with a type of data.</typeparam>
public abstract class ExecutionContext<TData> : IDisposable
    where TData : class
{
    /// <summary>
    /// Thread which execute the class
    /// </summary>
    public int Id => Context.IdThread;

    private ContextRun _context { get; } = new();

    /// <summary>
    /// Context in execution
    /// </summary>
    internal ContextRun Context => _context;

    /// <summary>
    /// Method checks state from current execution
    /// </summary>
    /// <exception cref="PendingRequestException">Generates exception when has pending request</exception>
    protected void CheckState()
    {
        if (_context.RequestStatus == ContextRunEnum.Paused || 
            _context.RequestStatus != ContextRunEnum.Disposed)
            throw new PendingRequestException();
    }

    /// <summary>
    /// Execution
    /// </summary>
    /// <param name="data">Data to execute</param>
    public abstract ExecutionResult Execute(TData data);

    /// <summary>
    /// Dispose resources from instance
    /// </summary>
    public virtual void Dispose()
    {
    }
}