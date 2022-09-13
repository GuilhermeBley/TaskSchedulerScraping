using System.Collections.Concurrent;
using TaskSchedulerScraping.Scraper.Results.Models;
using TaskSchedulerScraping.Scraper.Results.Context;
using TaskSchedulerScraping.Scraper.Results;

namespace TaskSchedulerScraping.Scraper.Model;

/// <summary>
/// Class run executions with initial data to search
/// </summary>
/// <typeparam name="TExecutionContext">Context to execution</typeparam>
/// <typeparam name="TData">Initial data to execute</typeparam>
public sealed class ModelScraper<TExecutionContext, TData> : IModelScraper, IDisposable
    where TData : class
    where TExecutionContext : IExecutionContext<TData>
{
    /// <summary>
    /// Finished execution list
    /// </summary>
    private readonly BlockingCollection<ResultBase<Exception?>> _endExec = new();

    /// <summary>
    /// Private currently status of execution
    /// </summary>
    private readonly ModelScraperStatus _status = new();

    /// <summary>
    /// Identifier generates on instance
    /// </summary>
    private readonly Guid _idScraper = Guid.NewGuid();

    /// <summary>
    /// Concurrent list of execution context
    /// </summary>
    private readonly Func<TExecutionContext> _getContext;

    /// <summary>
    /// FIFO of searches to do
    /// </summary>
    private readonly ConcurrentQueue<TData> _searchData;

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
    /// Concurrent list of execution context
    /// </summary>
    private BlockingCollection<TExecutionContext> _contexts { get; } = new();

    /// <summary>
    /// Scraping to execute
    /// </summary>
    private int _countScraper { get; }

    /// <inheritdoc cref="_countScraper" path="*"/>
    public int CountScraper => _countScraper;

    /// <summary>
    /// Unique Guid
    /// </summary>
    public Guid IdScraper => _idScraper;

    /// <summary>
    /// It is invoked when all workers finished
    /// </summary>
    public Action<IEnumerable<ResultBase<Exception?>>>? WhenAllWorksEnd;

    /// <summary>
    /// It is invoked when the data have searched with success or no.
    /// </summary>
    public Action<ResultBase<TData?>>? WhenDataFinished;

    public ModelScraper(
        int countScraper,
        Func<TExecutionContext> getContext,
        Func<IEnumerable<TData>> getData)
    {
        _countScraper = countScraper;
        _getContext = getContext;
        _searchData =
            new ConcurrentQueue<TData>(
                getData.Invoke() ?? throw new ArgumentNullException(nameof(getData))
            );
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

    public ResultBase<RunModel> Run()
    {
        if (_running == true)
            return ResultBase<RunModel>.GetWithError(new RunModel(RunModelEnum.AlreadyExecuted, _countScraper, "In process to start."));

        try
        {
            _running = true;

            if (_status.IsDisposed())
            {
                return ResultBase<RunModel>.GetWithError(new RunModel(RunModelEnum.Disposed, _countScraper, "Already disposed."));
            }

            if (_status.Status != ScraperStatusEnum.Starting)
            {
                return ResultBase<RunModel>.GetWithError(new RunModel(RunModelEnum.AlreadyExecuted, _countScraper, "Already started."));
            }

            for (int indexScraper = 0; indexScraper < _countScraper; indexScraper++)
            {
                var thread =
                    new Thread(() =>
                    {
                        Exception? exceptionEnd = null;
                        try
                        {
                            var executionContext = _getContext.Invoke();
                            _contexts.Add(executionContext);
                            using (executionContext)
                                RunSearch(executionContext);
                        }
                        catch(Exception e)
                        {
                            exceptionEnd = e;
                        }
                        finally
                        {
                            if (exceptionEnd is null)
                                _endExec.Add(ResultBase<Exception?>.GetSucess(exceptionEnd));
                            else
                                _endExec.Add(ResultBase<Exception?>.GetWithError(exceptionEnd));

                            if (IsFinished())
                            {
                                WhenAllWorksEnd?.Invoke(_endExec);
                            }
                        }
                    });

                thread.Start();
            };

            _status.SetStatus(ScraperStatusEnum.Running);

            return ResultBase<RunModel>.GetSucess(new RunModel(RunModelEnum.OkRequest, _countScraper));
        }
        catch (Exception e)
        {
            return ResultBase<RunModel>.GetWithError(new RunModel(RunModelEnum.FailedRequest, _countScraper, e.Message));
        }
        finally
        {
            _running = false;
        }

    }

    public async Task<ResultBase<StopModel>> StopAsync()
    {
        if (_status.IsDisposed())
        {
            await Task.CompletedTask;
            return ResultBase<StopModel>.GetWithError(new StopModel(StopModelEnum.Failed, "Already disposed"));
        }

        if (_disposing)
            return ResultBase<StopModel>.GetSucess(new StopModel(StopModelEnum.InProcess));

        _disposing = true;

        try
        {
            return await DisposeAsync();
        }
        finally
        {
            _disposing = false;
        }
    }

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

        while (!_contexts.All(context => context.Context.IsCurrentContextEqualsTheRequested() || context.Context.IsDisposed()))
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
    /// each worker thread execute this method
    /// </summary>
    /// <param name="executionContext">Context execution</param>
    /// <exception cref="ArgumentNullException"><paramref name="executionContext"/></exception>
    private void RunSearch(TExecutionContext executionContext)
    {
        var context = executionContext.Context ?? throw new ArgumentNullException(nameof(executionContext.Context));

        if (context.RequestStatus == ContextRunEnum.Disposed)
        {
            context.SetCurrentStatusWithException(
                new ObjectDisposedException(nameof(executionContext))
            );
            executionContext.Dispose();
            return;
        }
        if (context.RequestStatus == ContextRunEnum.Paused)
        {
            context.SetCurrentStatus(ContextRunEnum.Paused);
            Thread.Sleep(250);
            RunSearch(executionContext);
            return;
        }

        context.SetCurrentStatus(ContextRunEnum.Running);

        var hasData = _searchData.TryDequeue(out TData? dataOut);
        if (!hasData)
        {
            context.SetCurrentStatusFinished();
            executionContext.Dispose();
            return;
        }

        var searched = false;
        try
        {
            executionContext.Execute(dataOut!);

            searched = true;
            WhenDataFinished?.Invoke(ResultBase<TData?>.GetSucess(dataOut));
        }
        catch (ObjectDisposedException)
        {
            context.SetCurrentStatusWithException(
                new ObjectDisposedException(nameof(executionContext))
            );
            executionContext.Dispose();
            return;
        }
        finally
        {
            if (!searched && hasData)
            {
                _searchData.Enqueue(dataOut!);
                WhenDataFinished?.Invoke(ResultBase<TData?>.GetWithError(dataOut));
            }
        }

        if (_searchData.Any())
        {
            RunSearch(executionContext);
            return;
        }

        context.SetCurrentStatusFinished();
        executionContext.Dispose();
    }

    /// <summary>
    /// Checks if all of the executions are ended.
    /// </summary>
    /// <returns>true : all finished, false : in progress or isn't running</returns>
    private bool IsFinished()
    {
        if (_status.Status == ScraperStatusEnum.Starting)
            return false;

        if (_countScraper != _endExec.Count)
            return false;

        return true;
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
