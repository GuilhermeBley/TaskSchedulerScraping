using System.Collections.Concurrent;
using TaskSchedulerScraping.Scraper.Results.Models;
using TaskSchedulerScraping.Scraper.Results;

namespace TaskSchedulerScraping.Scraper.Model;

/// <summary>
/// Class run executions with initial data to search
/// </summary>
/// <typeparam name="TExecutionContext">Context to execution</typeparam>
/// <typeparam name="TData">Initial data to execute</typeparam>
public sealed class ModelScraper<TExecutionContext, TData> : IModelScraper, IDisposable
    where TData : class
    where TExecutionContext : ExecutionContext<TData>
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
    /// Function to get data to search
    /// </summary>
    private readonly Func<Task<IEnumerable<TData>>> _getData;

    /// <summary>
    /// Concurrent list of execution context
    /// </summary>
    private readonly BlockingCollection<TExecutionContext> _contexts = new();

    /// <summary>
    /// It is invoked when all workers finished
    /// </summary>
    private readonly Action<IEnumerable<ResultBase<Exception?>>>? _whenAllWorksEnd;

    /// <summary>
    /// It is invoked when the data have searched with success or no.
    /// </summary>
    private readonly Action<ResultBase<TData>>? _whenDataFinished;

    /// <summary>
    /// Called when exception occurs in a execution
    /// </summary>
    private readonly Func<Exception, TData, ExecutionResult>? _whenOccursException;

    /// <summary>
    /// Manual reset event
    /// </summary>
    private readonly ManualResetEvent _mre = new(true);

    /// <summary>
    /// Cancellation Token
    /// </summary>
    private CancellationTokenSource _cts = new();

    /// <summary>
    /// FIFO of searches to do
    /// </summary>
    private ConcurrentQueue<TData> _searchData = new();

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
    private int _countScraper { get; }

    /// <inheritdoc cref="_countScraper" path="*"/>
    public int CountScraper => _countScraper;

    /// <summary>
    /// Unique Guid
    /// </summary>
    public Guid IdScraper => _idScraper;

    /// <inheritdoc/>
    public ModelStateEnum State => _status.State;

    /// <summary>
    /// Instance of type <see cref="ModelScraper"/>
    /// </summary>
    /// <param name="countScraper">Quantity scrappers to run</param>
    /// <param name="getContext">Func to get new context</param>
    /// <param name="getData">Func to get all data to search</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public ModelScraper(
        int countScraper,
        Func<TExecutionContext> getContext,
        Func<Task<IEnumerable<TData>>> getData)
    {
        if (countScraper < 1)
            throw new ArgumentOutOfRangeException($"{nameof(countScraper)} should be more than zero.");

        _countScraper = countScraper;
        _getContext = getContext;
        _getData = getData;
    }

    /// <inheritdoc path="*"/>
    public ModelScraper(
        int countScraper,
        Func<TExecutionContext> getContext,
        Func<Task<IEnumerable<TData>>> getData,
        Func<Exception, TData, ExecutionResult>? whenOccursException = null,
        Action<ResultBase<TData>>? whenDataFinished = null,
        Action<IEnumerable<ResultBase<Exception?>>>? whenAllWorksEnd = null)
        : this(countScraper, getContext, getData)
    {
        _whenOccursException = whenOccursException;
        _whenDataFinished = whenDataFinished;
        _whenAllWorksEnd = whenAllWorksEnd;
    }

    /// <summary>
    /// Use <see cref="StopAsync"/>
    /// </summary>
    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Request stop and wait async
    /// </summary>
    /// <remarks>
    ///     <para>Cancel running or pause with token <see cref="_cts"/></para>
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token to cancel wait pause</param>
    /// <returns>PauseModel ResultBase</returns>
    public async Task<ResultBase<PauseModel>> PauseAsync(bool pause = true, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_status.IsDisposed())
        {
            await Task.CompletedTask;
            return ResultBase<PauseModel>.GetWithError(new PauseModel(PauseModelEnum.Failed, "Already disposed"));
        }

        if (_status.State != ModelStateEnum.Running && _status.State != ModelStateEnum.Paused)
            return ResultBase<PauseModel>.GetWithError(
                new PauseModel(PauseModelEnum.Failed, $"In state {Enum.GetName(typeof(ModelStateEnum), _status.State)} isn't allowed pause or unpause."));

        if (_running)
            return ResultBase<PauseModel>.GetWithError(new PauseModel(PauseModelEnum.Failed, "Running in process."));

        if (_disposing)
            return ResultBase<PauseModel>.GetWithError(new PauseModel(PauseModelEnum.Failed, "Disposing."));

        if (_pausing)
            return ResultBase<PauseModel>.GetSucess(new PauseModel(PauseModelEnum.InProcess));

        _pausing = true;
        try
        {
            if (pause && _status.State == ModelStateEnum.Paused)
                return ResultBase<PauseModel>.GetSucess(new PauseModel(PauseModelEnum.Paused));

            if (!pause && _status.State == ModelStateEnum.Running)
                return ResultBase<PauseModel>.GetSucess(new PauseModel(PauseModelEnum.Running));

            if (pause)
            {
                try
                {
                    _cts.Cancel();
                    _mre.Reset();
                    return await WaitPauseAsync(cancellationToken);
                }
                finally
                {
                    _cts.Dispose();
                }
            }

            if (!pause)
            {
                _cts = new();
                _mre.Set();
                return await WaitUnPauseAsync(cancellationToken);
            }

            else
                return ResultBase<PauseModel>.GetWithError(new PauseModel(PauseModelEnum.Failed, "Option not finded."));
        }
        finally
        {
            _pausing = false;
        }
    }

    public async Task<ResultBase<RunModel>> Run()
    {
        if (_running == true)
            return ResultBase<RunModel>.GetWithError(new RunModel(RunModelEnum.AlreadyExecuted, _countScraper, "In process to start."));

        if (_status.IsDisposed())
        {
            return ResultBase<RunModel>.GetWithError(new RunModel(RunModelEnum.Disposed, _countScraper, "Already disposed."));
        }

        if (_status.State != ModelStateEnum.NotRunning)
        {
            return ResultBase<RunModel>.GetWithError(new RunModel(RunModelEnum.AlreadyExecuted, _countScraper, "Already started."));
        }

        try
        {
            _running = true;

            _mre.Reset();

            _searchData = new ConcurrentQueue<TData>(await _getData.Invoke());

            for (int indexScraper = 0; indexScraper < _countScraper; indexScraper++)
            {
                var thread =
                    new Thread(() =>
                    {
                        _mre.WaitOne();
                        try
                        {
                            _endExec.Add(
                                RunExecute()
                            );
                        }
                        finally
                        {
                            if (IsFinished())
                            {
                                _whenAllWorksEnd?.Invoke(_endExec);
                                _status.SetStatus(ModelStateEnum.Disposed);
                                if (!_cts.IsCancellationRequested)
                                    _cts.Cancel();
                                _cts.Dispose();
                            }
                        }

                    });

                thread.Start();
            };

            _status.SetStatus(ModelStateEnum.Running);

            return ResultBase<RunModel>.GetSucess(new RunModel(RunModelEnum.OkRequest, _countScraper));
        }
        catch (Exception e)
        {
            return ResultBase<RunModel>.GetWithError(new RunModel(RunModelEnum.FailedRequest, _countScraper, e.Message));
        }
        finally
        {
            _mre.Set();
            _running = false;
        }

    }

    /// <summary>
    /// Request stop and wait async
    /// </summary>
    /// <remarks>
    ///     <para>Cancel wait with token <see cref="_cts"/></para>
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token to cancel wait stop</param>
    /// <returns>StopModel ResultBase</returns>
    public async Task<ResultBase<StopModel>> StopAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_status.IsDisposed())
        {
            await Task.CompletedTask;
            return ResultBase<StopModel>.GetSucess(new StopModel(StopModelEnum.Stoped, "Already disposed"));
        }

        if (_status.State != ModelStateEnum.Running)
            return ResultBase<StopModel>.GetWithError(
                new StopModel(StopModelEnum.Failed, $"In State {Enum.GetName(typeof(ModelStateEnum), _status.State)} isn't allowed stop."));

        if (_running)
            return ResultBase<StopModel>.GetWithError(new StopModel(StopModelEnum.Failed, "Running in process."));

        if (_pausing)
            return ResultBase<StopModel>.GetWithError(new StopModel(StopModelEnum.Failed, "Pausing in process."));

        if (_disposing)
            return ResultBase<StopModel>.GetSucess(new StopModel(StopModelEnum.InProcess));

        _disposing = true;

        try
        {
            if (!_cts.IsCancellationRequested)
                _cts.Cancel();
            return await WaitDisposeAsync(cancellationToken);
        }
        finally
        {
            _disposing = false;
            _cts.Dispose();
        }
    }

    /// <summary>
    /// Only one Thread should be acess this method
    /// </summary>
    /// <remarks>
    ///     <para></para>
    /// </remarks>
    private ResultBase<Exception?> RunExecute()
    {
        Exception? exceptionEnd = null;
        var executionContext = _getContext.Invoke();
        try
        {
            _contexts.Add(executionContext);
            using (executionContext)
                RunLoopSearch(executionContext);

            executionContext.Context.SetCurrentStatusFinished();
            return ResultBase<Exception?>.GetSucess(exceptionEnd);
        }
        catch (Exception e)
        {
            exceptionEnd = e;
            executionContext.Context.SetCurrentStatusWithException(exceptionEnd);
            return ResultBase<Exception?>.GetWithError(exceptionEnd);
        }
    }

    /// <summary>
    /// Pause and set all of the execute context to pause
    /// </summary>
    /// <remarks>
    ///     <para>Status set to Paused</para>
    /// </remarks>
    private async Task<ResultBase<PauseModel>> WaitPauseAsync(CancellationToken cancellationToken = default)
    {
        foreach (var contextInfo in _contexts.Select(context => context.Context))
        {
            contextInfo.SetRequestStatus(ContextRunEnum.Paused);
        }

        while (!_contexts.All(context => context.Context.IsCurrentContextEqualsTheRequested() || context.Context.IsDisposed()))
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Delay(300);
        }

        _status.SetStatus(ModelStateEnum.Paused);

        return ResultBase<PauseModel>.GetSucess(new PauseModel(PauseModelEnum.Paused));
    }

    /// <summary>
    /// Unpause and set all of the execute context to running
    /// </summary>
    /// <remarks>
    ///     <para>Status set to running</para>
    /// </remarks>
    private async Task<ResultBase<PauseModel>> WaitUnPauseAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var contextInfo in _contexts.Select(context => context.Context))
        {
            contextInfo.SetRequestStatus(ContextRunEnum.Running);
        }

        while (!_contexts.All(context => context.Context.IsCurrentContextEqualsTheRequested() || context.Context.IsDisposed()))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(300);
        }

        _status.SetStatus(ModelStateEnum.Running);

        return ResultBase<PauseModel>.GetSucess(new PauseModel(PauseModelEnum.Running));
    }

    /// <summary>
    /// Dispose and set all of the execute context to disposed
    /// </summary>
    /// <returns></returns>
    private async Task<ResultBase<StopModel>> WaitDisposeAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        foreach (var contextInfo in _contexts.Select(context => context.Context))
        {
            if (!contextInfo.IsDisposed())
                contextInfo.SetRequestStatus(ContextRunEnum.Disposed);
        }

        while (!_contexts.All(context => context.Context.IsCurrentContextEqualsTheRequested() || context.Context.IsDisposed()))
        {
            token.ThrowIfCancellationRequested();
            await Task.Delay(300);
        }

        _status.SetStatus(ModelStateEnum.Disposed);

        return ResultBase<StopModel>.GetSucess(new StopModel(StopModelEnum.Stoped));
    }

    /// <summary>
    /// each worker thread execute this method
    /// </summary>
    /// <param name="executionContext">Context execution</param>
    /// <exception cref="ArgumentNullException"><paramref name="executionContext"/></exception>
    private void RunLoopSearch(TExecutionContext executionContext, TData? dataToSearch = null)
    {
        var context = executionContext.Context ?? throw new ArgumentNullException(nameof(executionContext.Context));

        if (context.RequestStatus == ContextRunEnum.Disposed &&
            _cts.IsCancellationRequested)
        {
            context.SetCurrentStatusWithException(
                new ObjectDisposedException(nameof(executionContext))
            );
            return;
        }
        if (context.RequestStatus == ContextRunEnum.Paused &&
            _cts.IsCancellationRequested)
        {
            context.SetCurrentStatus(ContextRunEnum.Paused);
            _mre.WaitOne();
            RunLoopSearch(executionContext, dataToSearch);
            return;
        }

        context.SetCurrentStatus(ContextRunEnum.Running);

        if (dataToSearch is null)
            _searchData.TryDequeue(out dataToSearch);

        if (dataToSearch is null)
        {
            context.SetCurrentStatusFinished();
            return;
        }

        ExecutionResult executionResult;
        Exception? exception = null;
        try
        {
            executionResult =
                executionContext.Execute(dataToSearch, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            executionResult = ExecutionResult.RetrySame("Pending request");
        }
        catch (Exception e)
        {
            exception = e;
            executionResult =
                _whenOccursException is null ? ExecutionResult.ThrowException(e) :
                _whenOccursException.Invoke(e, dataToSearch);
        }

        if (executionResult.ActionToNextData == ExecutionResultEnum.Next)
        {
            _whenDataFinished?.Invoke(ResultBase<TData>.GetSucess(dataToSearch));
            RunLoopSearch(executionContext, null);
            return;
        }

        if (executionResult.ActionToNextData == ExecutionResultEnum.RetrySame)
        {
            _whenDataFinished?.Invoke(ResultBase<TData>.GetWithError(dataToSearch));
            RunLoopSearch(executionContext, dataToSearch);
            return;
        }

        if (executionResult.ActionToNextData == ExecutionResultEnum.RetryOther)
        {
            _searchData.Enqueue(dataToSearch);
            _whenDataFinished?.Invoke(ResultBase<TData>.GetWithError(dataToSearch));
            RunLoopSearch(executionContext, null);
            return;
        }

        if (executionResult.ActionToNextData == ExecutionResultEnum.ThrowException)
        {
            _searchData.Enqueue(dataToSearch);
            _whenDataFinished?.Invoke(ResultBase<TData>.GetWithError(dataToSearch));
            context.SetCurrentStatusWithException(exception);
            return;
        }

        context.SetCurrentStatusFinished();
    }

    /// <summary>
    /// Checks if all of the executions are ended.
    /// </summary>
    /// <returns>true : all finished, false : in progress or isn't running</returns>
    private bool IsFinished()
    {
        if (_countScraper != _endExec.Count)
            return false;

        return true;
    }

    /// <summary>
    /// Status model scrapper
    /// </summary>
    private class ModelScraperStatus
    {
        private ModelStateEnum _state = ModelStateEnum.NotRunning;
        public ModelStateEnum State => _state;

        /// <summary>
        /// Set <see cref="State"/>
        /// </summary>
        /// <param name="state">Status to set</param>
        /// <returns>result base</returns>
        public ResultBase<string> SetStatus(ModelStateEnum state)
        {
            if (_state == ModelStateEnum.Disposed)
                return ResultBase<string>.GetWithError("Already disposed.");

            if (state == ModelStateEnum.NotRunning)
                return ResultBase<string>.GetWithError("Already started.");

            if (state == _state)
                return ResultBase<string>.GetSucess($"Ok");

            _state = state;
            return ResultBase<string>.GetSucess("Ok");
        }

        /// <summary>
        /// Check if is disposed
        /// </summary>
        public bool IsDisposed()
        {
            if (_state == ModelStateEnum.Disposed)
                return true;

            return false;
        }
    }
}
