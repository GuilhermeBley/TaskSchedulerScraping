using TaskSchedulerScraping.Scraper.Results.Context;

namespace TaskSchedulerScraping.Scraper.Model;

/// <summary>
/// Context to execute quest
/// </summary>
/// <typeparam name="TData">Works with a type of data.</typeparam>
public abstract class Quest<TData> : IDisposable
    where TData : class
{
    /// <summary>
    /// Context in execution
    /// </summary>
    private ContextRun _context { get; } = new();

    /// <inheritdoc cref="_context" path="*"/>
    internal ContextRun Context => _context;

    /// <summary>
    /// Thread which execute the class
    /// </summary>
    public int Id => Context.IdThread;

    /// <summary>
    /// Execution
    /// </summary>
    /// <param name="data">Data to execute</param>
    public abstract QuestResult Execute(TData data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispose resources from instance
    /// </summary>
    public virtual void Dispose()
    {
    }
}