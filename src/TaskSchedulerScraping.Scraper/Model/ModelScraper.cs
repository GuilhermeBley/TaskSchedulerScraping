using System.Collections.Concurrent;
using TaskSchedulerScraping.Scraper.Results.Models;
using TaskSchedulerScraping.Scraper.Results.Context;
using TaskSchedulerScraping.Scraper.Results;

namespace TaskSchedulerScraping.Scraper.Model;

public abstract class ModelScraper<TData> : IModelScraper, IDisposable
    where TData : class
{
    private readonly ModelScraperStatus _status = new();

    private readonly Guid _idScraper = Guid.NewGuid();

    /// <summary>
    /// Concurrent list of execution context
    /// </summary>
    private readonly IEnumerable<IExecutionContext<TData>> _contexts;

    /// <summary>
    /// FIFO of searches to do
    /// </summary>
    private readonly ConcurrentQueue<TData> _searches;

    /// <summary>
    /// Thread in pausing
    /// </summary>
    private bool _pausing = false;

    /// <summary>
    /// Thread in running
    /// </summary>
    private bool _running = false;

    /// <summary>
    /// Thread in disposing
    /// </summary>
    private bool _disposing = false;

    /// <summary>
    /// Scraping to execute
    /// </summary>
    protected abstract int _countScraper { get; }

    /// <inheritdoc cref="_countScraper" path="*"/>
    public int CountScraper => _countScraper;

    /// <summary>
    /// Unique Guid
    /// </summary>
    public Guid IdScraper => _idScraper;

    /// <summary>
    /// Name of scraper
    /// </summary>
    public abstract string ModelScraperName { get; }

    public ModelScrapper()
    {

    }

    /// <summary>
    /// Use <see cref="StopAsync"/>
    /// </summary>
    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
    }

    public async Task<ResultBase<PauseModel>> PauseAsync(bool pause = true)
    {
        if (_pausing)
            return ResultBase<PauseModel>.GetSucess(new PauseModel(PauseModelEnum.InProcess));

        _pausing = true;
        try
        {
            if (_status.IsDisposed())
            {
                await Task.CompletedTask;
                return ResultBase<PauseModel>.GetWithError(new PauseModel(PauseModelEnum.Failed, "already disposed."));
            }

            if (pause && _status.Status == ScraperStatusEnum.Paused)
                return ResultBase<PauseModel>.GetSucess(new PauseModel(PauseModelEnum.Paused));

            if (!pause && _status.Status == ScraperStatusEnum.Running)
                return ResultBase<PauseModel>.GetSucess(new PauseModel(PauseModelEnum.Running));

            if (pause)
                return await PauseAsync();

            if (!pause)
                return await UnPauseAsync();

            else
                return ResultBase<PauseModel>.GetWithError(new PauseModel(PauseModelEnum.Failed, "Option not finded."));
        }
        finally
        {
            _pausing = false;
        }
    }

    public ResultBase<RunModel> RunAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<ResultBase<StopModel>> StopAsync()
    {
        if (_status.IsDisposed())
        {
            await Task.CompletedTask;
            return ResultBase<StopModel>.GetWithError(new StopModel(StopModelEnum.Failed, "Already disposed"));
        }

        if (_disposing)
            return ResultBase<StopModel>.GetSucess(new StopModel(StopModelEnum.Stopping))

        _disposing = true;

        try{

        }finally
        {
            _disposing = false;
        }
        return await DisposeAsync();
    }

    protected abstract IExecutionContext<TData> GetExecutionContext();
    protected abstract IEnumerable<TData> GetExecutionData();

    
    /// <summary>
    /// Pause and set all of the execute context to pause
    /// </summary>
    /// <remarks>
    ///     <para>Status set to Paused</para>
    /// </remarks>
    private async Task<ResultBase<PauseModel>> PauseAsync()
    {
        foreach (var contextInfo in _contexts.Select(context => context.Context))
        {
            contextInfo.SetRequestStatus(ContextRunEnum.Paused);
        }

        while (!_contexts.All(context => context.Context.IsCurrentContextEqualsTheRequested()  || context.Context.IsDisposed()))
        {
            await Task.Delay(300);
        }

        _status.SetStatus(ScraperStatusEnum.Paused);

        return ResultBase<PauseModel>.GetSucess(new PauseModel(PauseModelEnum.Paused));
    }

    /// <summary>
    /// Unpause and set all of the execute context to running
    /// </summary>
    /// <remarks>
    ///     <para>Status set to running</para>
    /// </remarks>
    private async Task<ResultBase<PauseModel>> UnPauseAsync()
    {
        foreach (var contextInfo in _contexts.Select(context => context.Context))
        {
            contextInfo.SetRequestStatus(ContextRunEnum.Running);
        }

        while (!_contexts.All(context => context.Context.IsCurrentContextEqualsTheRequested() || context.Context.IsDisposed()))
        {
            await Task.Delay(300);
        }

        _status.SetStatus(ScraperStatusEnum.Running);

        return ResultBase<PauseModel>.GetSucess(new PauseModel(PauseModelEnum.Running));
    }

    /// <summary>
    /// Dispose and set all of the execute context to disposed
    /// </summary>
    /// <returns></returns>
    private async Task<ResultBase<StopModel>> DisposeAsync()
    {
        foreach (var contextInfo in _contexts.Select(context => context.Context))
        {
            if (!contextInfo.IsDisposed())
                contextInfo.SetRequestStatus(ContextRunEnum.Disposed);
        }

        while (!_contexts.All(context => context.Context.IsCurrentContextEqualsTheRequested() || context.Context.IsDisposed()))
        {
            await Task.Delay(300);
        }

        _status.SetStatus(ScraperStatusEnum.Disposed);

        return ResultBase<StopModel>.GetSucess(new StopModel(StopModelEnum.Stoped));
    }

    /// <summary>
    /// Status model scrapper
    /// </summary>
    private class ModelScraperStatus
    {
        private ScraperStatusEnum _status = ScraperStatusEnum.Starting;
        public ScraperStatusEnum Status => _status;

        /// <summary>
        /// Set <see cref="Status"/>
        /// </summary>
        /// <param name="status">Status to set</param>
        /// <returns>result base</returns>
        public ResultBase<string> SetStatus(ScraperStatusEnum @status)
        {
            if (_status == ScraperStatusEnum.Disposed)
                return ResultBase<string>.GetWithError("Already disposed.");

            if (@status == ScraperStatusEnum.Starting)
                return ResultBase<string>.GetWithError("Already started.");

            if (@status == _status)
                return ResultBase<string>.GetSucess($"Ok");

            _status = @status;
            return ResultBase<string>.GetSucess("Ok");
        }

        /// <summary>
        /// Check if is disposed
        /// </summary>
        public bool IsDisposed()
        {
            if (_status == ScraperStatusEnum.Disposed)
                return true;

            return false;
        }
    }

    /// <summary>
    /// Types of states
    /// </summary>
    private enum ScraperStatusEnum : sbyte
    {
        Starting = 0,
        Running = 1,
        Paused = 2,
        Disposed = 3
    }
}
