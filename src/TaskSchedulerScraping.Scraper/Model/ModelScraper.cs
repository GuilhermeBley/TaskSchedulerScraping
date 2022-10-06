using System.Collections.Concurrent;
using TaskSchedulerScraping.Scraper.Results.Models;
using TaskSchedulerScraping.Scraper.Results;

namespace TaskSchedulerScraping.Scraper.Model;

/// <summary>
/// Class run executions with initial data to search
/// </summary>
/// <remarks>
///     <para>Class works with a quantity of data predefined</para>
/// </remarks>
/// <typeparam name="TExecutionContext">Context to execution</typeparam>
/// <typeparam name="TData">Initial data to execute</typeparam>
public class ModelScraper<TExecutionContext, TData> : IModelScraper
    where TData : class
    where TExecutionContext : Quest<TData>
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
    /// <remarks>
    ///     <para>Last thread execute this method when all of search is disposed</para>
    /// </remarks>
    private readonly Action<IEnumerable<ResultBase<Exception?>>>? _whenAllWorksEnd;

    /// <summary>
    /// It is invoked when the data have searched with success or no.
    /// </summary>
    /// <remarks>
    ///     <para>When a data was finished, the thread call this function</para>
    /// </remarks>
    private readonly Action<ResultBase<TData>>? _whenDataFinished;

    /// <summary>
    /// Called when exception occurs in a execution
    /// </summary>
    /// <remarks>
    ///     <para>Thread which throw exception call this to know your next behavior</para>
    /// </remarks>
    private readonly Func<Exception, TData, QuestResult>? _whenOccursException;

    /// <summary>
    /// Called when all data was collected to run.
    /// </summary>
    /// <remarks>
    ///     <para>When data to search was collected in Run, this method is called</para>
    /// </remarks>
    private readonly Action<IEnumerable<TData>>? _whenDataWasCollected;

    /// <summary>
    /// Manual reset event
    /// </summary>
    private readonly ManualResetEvent _mrePause = new(true);

    /// <summary>
    /// Manual reset event
    /// </summary>
    private readonly ManualResetEvent _mreWaitProcessing = new(true);

    /// <summary>
    /// Cancellation Token
    /// </summary>
    private CancellationTokenSource _cts = new();

    /// <summary>
    /// FIFO of searches to do
    /// </summary>
    private ConcurrentQueue<TData> _searchData = new();

    /// <summary>
    /// Object can be used to lock in multithread requests
    /// </summary>
    private readonly object _stateLock = new();

    /// <summary>
    /// Scraping to execute
    /// </summary>
    private int _countScraper { get; }
    
    /// <summary>
    /// Run at
    /// </summary>
    private DateTime? _dtRun = null;

    /// <inheritdoc cref="_countScraper" path="*"/>
    public int CountScraper => _countScraper;

    /// <summary>
    /// Unique Guid
    /// </summary>
    public Guid IdScraper => _idScraper;

    /// <inheritdoc/>
    public ModelStateEnum State => _status.State;

    /// <summary>
    /// Run at
    /// </summary>
    public DateTime? DtRun => _dtRun;

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

    /// <summary>
    /// Instance with actions and functions
    /// </summary>
    /// <param name="countScraper">Quantity scrappers to run</param>
    /// <param name="getContext">Func to get new context</param>
    /// <param name="getData">Func to get all data to search</param>
    /// <param name="whenOccursException">Thread which throw exception call this to know your next behavior</param>
    /// <param name="whenDataFinished">When a data was finished, the thread call this function</param>
    /// <param name="whenAllWorksEnd">Last thread execute this method when all of search is disposed</param>
    /// <param name="whenDataWasCollected">When data to search was collected in Run, this method is called</param>
    public ModelScraper(
        int countScraper,
        Func<TExecutionContext> getContext,
        Func<Task<IEnumerable<TData>>> getData,
        Func<Exception, TData, QuestResult>? whenOccursException = null,
        Action<ResultBase<TData>>? whenDataFinished = null,
        Action<IEnumerable<ResultBase<Exception?>>>? whenAllWorksEnd = null,
        Action<IEnumerable<TData>>? whenDataWasCollected = null)
        : this(countScraper, getContext, getData)
    {
        _whenOccursException = whenOccursException;
        _whenDataFinished = whenDataFinished;
        _whenAllWorksEnd = whenAllWorksEnd;
        _whenDataWasCollected = whenDataWasCollected;
    }

    /// <summary>
    /// Disposable, cancel all threads in execution
    /// </summary>
    public void Dispose()
    {
        TryRequestStop();
    }

    /// <summary>
    /// Async disposable, cancel all threads in execution and wait cancel
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }

    /// <inheritdoc path="*"/>
    public async Task<ResultBase<PauseModel>> PauseAsync(bool pause = true, CancellationToken cancellationToken = default)
    {
        ModelStateEnum stateLocked;
        lock (_stateLock)
        {
            if (_status.IsDisposedOrDisposing())
            {
                return ResultBase<PauseModel>.GetWithError(new PauseModel(PauseModelEnum.Failed, "Already disposed"));
            }

            if (_status.IsInitializing())
            {
                return ResultBase<PauseModel>.GetWithError(new PauseModel(PauseModelEnum.Failed, "Not Initialized"));
            }

            stateLocked = _status.State;
        }

        if (pause && stateLocked == ModelStateEnum.WaitingPause)
        {
            await WaitStateAllContexts(ModelStateEnum.Paused, cancellationToken);
            return ResultBase<PauseModel>.GetSucess(new PauseModel(PauseModelEnum.InProcess));
        }

        if (!pause && stateLocked == ModelStateEnum.WaitingRunning)
        {
            await WaitStateAllContexts(ModelStateEnum.Running, cancellationToken);
            return ResultBase<PauseModel>.GetSucess(new PauseModel(PauseModelEnum.InProcess));
        }

        if (pause && _status.State == ModelStateEnum.Paused)
            return ResultBase<PauseModel>.GetSucess(new PauseModel(PauseModelEnum.Paused));

        if (!pause && _status.State == ModelStateEnum.Running)
            return ResultBase<PauseModel>.GetSucess(new PauseModel(PauseModelEnum.Running));

        if (pause)
        {
            try
            {
                lock (_stateLock)
                {
                    _mreWaitProcessing.Reset();

                    _cts.Cancel();
                    _mrePause.Reset();

                    SetStateAllContexts(ContextRunEnum.Paused);

                    _status.SetState(ModelStateEnum.WaitingPause);
                }
            }
            finally
            {
                _mreWaitProcessing.Set();
            }

            await WaitStateAllContexts(ModelStateEnum.Paused, cancellationToken);

            return ResultBase<PauseModel>.GetSucess(new PauseModel(PauseModelEnum.Paused));
        }

        if (!pause)
        {
            try
            {
                lock (_stateLock)
                {
                    _mreWaitProcessing.Reset();

                    _status.SetState(ModelStateEnum.WaitingRunning);

                    if (!_cts.IsCancellationRequested)
                        _cts.Cancel();
                    _cts.Dispose();
                    _cts = new();

                    SetStateAllContexts(ContextRunEnum.Running);
                }
            }
            finally
            {
                _mreWaitProcessing.Set();
                _mrePause.Set();
            }

            await WaitStateAllContexts(ModelStateEnum.Running, cancellationToken);

            return ResultBase<PauseModel>.GetSucess(new PauseModel(PauseModelEnum.Running));
        }

        else
            return ResultBase<PauseModel>.GetWithError(new PauseModel(PauseModelEnum.Failed, "Option not finded."));

    }

    /// <inheritdoc path="*"/>
    public async Task<ResultBase<RunModel>> Run()
    {
        _mreWaitProcessing.WaitOne();

        lock (_stateLock)
        {
            if (_status.IsDisposedOrDisposing())
            {
                return ResultBase<RunModel>.GetWithError(new RunModel(RunModelEnum.Disposed, _countScraper, "Already disposed."));
            }

            if (_status.State != ModelStateEnum.NotRunning)
            {
                return ResultBase<RunModel>.GetWithError(new RunModel(RunModelEnum.AlreadyExecuted, _countScraper, "Already started."));
            }

            lock (_stateLock)
                _status.SetState(ModelStateEnum.WaitingRunning);
        }

        try
        {
            var data = await _getData.Invoke();

            _searchData = new ConcurrentQueue<TData>(data);

            _whenDataWasCollected?.Invoke(data);
        }
        catch
        {
            _searchData = new();
            lock (_stateLock)
                _status.SetState(ModelStateEnum.NotRunning);
            throw;
        }

        try
        {
            _mreWaitProcessing.Reset();

            for (int indexScraper = 0; indexScraper < _countScraper; indexScraper++)
            {
                var thread =
                    new Thread(() =>
                    {
                        _mreWaitProcessing.WaitOne();
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
                                lock (_stateLock)
                                    _status.SetState(ModelStateEnum.Disposed);

                                try
                                {
                                    _whenAllWorksEnd?.Invoke(_endExec);
                                }
                                catch { }

                                if (!_cts.IsCancellationRequested)
                                    _cts.Cancel();
                                _cts.Dispose();
                            }
                        }

                    });

                thread.Start();
            };

            lock (_stateLock)
                _status.SetState(ModelStateEnum.Running);

            return ResultBase<RunModel>.GetSucess(new RunModel(RunModelEnum.OkRequest, _countScraper));
        }
        finally
        {
            _dtRun = DateTime.Now;
            _mreWaitProcessing.Set();
        }

    }

    /// <inheritdoc path="*"/>
    public async Task<ResultBase<StopModel>> StopAsync(CancellationToken cancellationToken = default)
    {
        if (_status.IsDisposed())
        {
            await Task.CompletedTask;
            return ResultBase<StopModel>.GetSucess(new StopModel(StopModelEnum.Stoped, "Already disposed"));
        }

        _mreWaitProcessing.WaitOne();

        TryRequestStop();

        if (_status.State == ModelStateEnum.WaitingDispose)
            await WaitStateAllContexts(ModelStateEnum.Disposed, cancellationToken);

        return ResultBase<StopModel>.GetSucess(new StopModel(StopModelEnum.Stoped));
    }

    /// <summary>
    /// This method request cancel to all quests
    /// </summary>
    /// <remarks>
    ///     <para></para>
    ///     <para>Unpause threads</para>
    ///     <para>Cancel Cancellation Token</para>
    ///     <para>Set all contexts to cancel</para>
    /// </remarks>
    private void TryRequestStop()
    {
        lock (_stateLock)
        {
            if (_status.IsDisposedOrDisposing())
            {
                return;
            }

            if (!_contexts.Any())
            {
                if (!_cts.IsCancellationRequested)
                    _cts.Cancel();

                _cts.Dispose();

                _status.SetState(ModelStateEnum.Disposed);

                return;
            }

            try
            {
                _mreWaitProcessing.Reset();

                _mrePause.Set();

                if (!_cts.IsCancellationRequested)
                    _cts.Cancel();

                SetStateAllContexts(ContextRunEnum.Disposed);

                _status.SetState(ModelStateEnum.WaitingDispose);
            }
            finally
            {
                _mreWaitProcessing.Set();
            }
        }
    }

    /// <summary>
    /// Only one Thread should be acess this method
    /// </summary>
    /// <remarks>
    ///     <para>Creates a new instance of <typeparamref name="TExecutionContext"/> to each thread which executes this method</para>
    /// </remarks>
    private ResultBase<Exception?> RunExecute()
    {
        Exception? exceptionEnd = null;
        TExecutionContext executionContext;
        try
        {
            executionContext = _getContext.Invoke();
        }
        catch (Exception e)
        {
            return ResultBase<Exception?>.GetWithError(
                    new InvalidOperationException($"Failed to create {nameof(TExecutionContext)}.", e)
                );
        }

        if (executionContext.Id != Thread.CurrentThread.ManagedThreadId)
            throw new ArgumentException($"Context doesn't executing in correct process. Check if ");

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
    /// each worker thread execute this method
    /// </summary>
    /// <param name="executionContext">Context execution</param>
    /// <exception cref="ArgumentNullException"><paramref name="executionContext"/></exception>
    private void RunLoopSearch(TExecutionContext executionContext, TData? dataToSearch = null)
    {
        _mreWaitProcessing.WaitOne();

        var context = executionContext.Context ?? throw new ArgumentNullException(nameof(executionContext.Context));

        if (context.RequestStatus == ContextRunEnum.Disposed)
        {
            context.SetCurrentStatusWithException(
                new ObjectDisposedException(nameof(executionContext))
            );
            return;
        }
        if (context.RequestStatus == ContextRunEnum.Paused)
        {
            context.SetCurrentStatus(ContextRunEnum.Paused);

            if (_status.State != ModelStateEnum.Paused &&
            _contexts.All(context => context.Context.CurrentStatus == ContextRunEnum.Paused || context.Context.IsDisposed()))
            {
                lock(_stateLock)
                    _status.SetState(ModelStateEnum.Paused);
            }

            _mrePause.WaitOne();
            RunLoopSearch(executionContext, dataToSearch);
            return;
        }

        context.SetCurrentStatus(ContextRunEnum.Running);

        if (_status.State != ModelStateEnum.Running &&
            _contexts.All(context => context.Context.CurrentStatus == ContextRunEnum.Running))
        {
            lock(_stateLock)
                _status.SetState(ModelStateEnum.Running);
        }


        if (dataToSearch is null)
            _searchData.TryDequeue(out dataToSearch);

        if (dataToSearch is null)
        {
            context.SetCurrentStatusFinished();
            return;
        }

        QuestResult executionResult;
        Exception? exception = null;
        try
        {
            executionResult =
                executionContext.Execute(dataToSearch, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            executionResult = QuestResult.RetrySame("Pending request");
        }
        catch (Exception e)
        {
            exception = e;
            executionResult =
                _whenOccursException is null ? QuestResult.ThrowException(e) :
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

        if (_searchData.Any())
        {
            _searchData.Enqueue(dataToSearch);
            _whenDataFinished?.Invoke(ResultBase<TData>.GetWithError(dataToSearch));
            RunLoopSearch(executionContext, null);
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
    /// Method checks if all running threads reach your requested state or disposed
    /// </summary>
    /// <param name="token">Token to cancel wait</param>
    private async Task WaitStateAllContexts(ModelStateEnum state, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        while (!_contexts.All(context => _status.State == state || _status.IsDisposed()))
        {
            token.ThrowIfCancellationRequested();
            await Task.Delay(300);
        }
    }

    /// <summary>
    /// Sets all states running if state isn't disposed.
    /// </summary>
    /// <param name="allContextToRequest">context to set</param>
    private void SetStateAllContexts(ContextRunEnum allContextToRequest)
    {
        if (_contexts is null)
            return;

        foreach (var contextInfo in _contexts.Select(context => context.Context))
        {
            if (!contextInfo.IsDisposed())
                contextInfo.SetRequestStatus(allContextToRequest);
        }
    }

    /// <summary>
    /// Status model scrapper
    /// </summary>
    private class ModelScraperStatus
    {
        private ModelStateEnum _state = ModelStateEnum.NotRunning;
        public ModelStateEnum State => GetLockedState();

        /// <summary>
        /// Object can be used to lock in multithread requests
        /// </summary>
        private readonly object _stateLock = new();

        /// <summary>
        /// Set a value with thread safe
        /// </summary>
        /// <param name="state">State to set</param>
        /// <param name="whileLocked">Action before setting, includes in safe processing</param>
        /// <returns></returns>
        public ResultBase<string> SetState(ModelStateEnum state)
        {
            lock (_stateLock)
            {
                if (_state == ModelStateEnum.Disposed)
                    return ResultBase<string>.GetWithError("Already disposed.");

                if (state == ModelStateEnum.NotRunning && _state != ModelStateEnum.WaitingRunning)
                    return ResultBase<string>.GetWithError("Already started.");

                if (state == _state)
                    return ResultBase<string>.GetSucess($"Ok");

                _state = state;
                return ResultBase<string>.GetSucess("Ok");
            }
        }

        /// <summary>
        /// Gets locked state
        /// </summary>
        /// <remarks>
        ///     <para>Use lock state</para>
        /// </remarks>
        private ModelStateEnum GetLockedState()
        {
            lock (_stateLock)
                return _state;
        }

        /// <summary>
        /// Check if is disposed
        /// </summary>
        public bool IsDisposed()
        {
            if (GetLockedState() == ModelStateEnum.Disposed)
                return true;

            return false;
        }

        /// <summary>
        /// Check if is NotRunning or WaitingRunning
        /// </summary>
        public bool IsInitializing()
        {
             var state = GetLockedState();
            if (state == ModelStateEnum.WaitingRunning
                || state == ModelStateEnum.NotRunning)
                return true;

            return false;
        }

        /// <summary>
        /// Check if is disposed or disposing
        /// </summary>
        public bool IsDisposedOrDisposing()
        {
            var state = GetLockedState();
            if (state == ModelStateEnum.WaitingDispose
                || state == ModelStateEnum.Disposed)
                return true;

            return false;
        }
    }
}
