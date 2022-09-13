namespace TaskSchedulerScraping.Scraper.Results.Context;

/// <summary>
/// Status of context actual of execution
/// </summary>
public sealed class ContextRun
{
    /// <summary>
    /// Private readonly id, should be unique
    /// </summary>
    private readonly int _id;

    /// <summary>
    /// Current status of context
    /// </summary>
    private ContextRunEnum _currentStatus = ContextRunEnum.Running;

    /// <summary>
    /// Requested status of context
    /// </summary>
    private ContextRunEnum _requestStatus = ContextRunEnum.Running;

    /// <summary>
    /// Exception generated with the disposed
    /// </summary>
    private Exception? _exception;

    /// <inheritdoc cref="_currentStatus" path="*"/>
    public ContextRunEnum CurrentStatus => _currentStatus;

    /// <inheritdoc cref="_requestStatus" path="*"/>
    public ContextRunEnum RequestStatus => _requestStatus;

    /// <inheritdoc cref="_exception" path="*"/>
    public Exception? Exception => _exception;

    /// <summary>
    /// Instance of <see cref="ContextRunEnum"/>
    /// </summary>
    public ContextRun()
    {
        _id = Thread.CurrentThread.ManagedThreadId;
    }

    /// <summary>
    /// Instance of <see cref="ContextRunEnum"/>
    /// </summary>
    /// <param name="id">identifier of <see cref="ContextRunEnum"/></param>
    internal ContextRun(int id)
    {
        _id = id;
    }

    /// <summary>
    /// Set the <see cref="RequestStatus"/>
    /// </summary>
    /// <remarks>
    ///     <para>Only acessible with the context thread</para>
    /// </remarks>
    /// <param name="requestStatus">The status to set</param>
    /// <returns><see cref="ResultBase"/></returns>
    internal ResultBase<RequestStatusEnum> SetRequestStatus(ContextRunEnum requestStatus)
    {
        if (Thread.CurrentThread.ManagedThreadId == _id)
            return ResultBase<RequestStatusEnum>.GetWithError(RequestStatusEnum.NotAllowed);

        if (IsStatusDiposed(_requestStatus))
            return ResultBase<RequestStatusEnum>.GetWithError(RequestStatusEnum.Disposed);

        if (requestStatus.Equals(_requestStatus))
            return ResultBase<RequestStatusEnum>.GetSucess(RequestStatusEnum.AlreadyRequested);

        if (requestStatus.Equals(ContextRunEnum.DisposedWithError) ||
            requestStatus.Equals(ContextRunEnum.DisposedWithError))
            return ResultBase<RequestStatusEnum>.GetWithError(RequestStatusEnum.NotAllowed);

        _requestStatus = requestStatus;

        return ResultBase<RequestStatusEnum>.GetSucess(RequestStatusEnum.Requested);
    }

    /// <summary>
    /// Set the <see cref="CurrentStatus"/>
    /// </summary>
    /// <remarks>
    ///     <para>Only acessible with the context thread</para>
    /// </remarks>
    /// <param name="requestStatus">The status to set</param>
    /// <returns><see cref="ResultBase"/></returns>
    internal ResultBase<RequestStatusEnum> SetCurrentStatus(ContextRunEnum requestStatus)
    {
        if (Thread.CurrentThread.ManagedThreadId != _id)
            return ResultBase<RequestStatusEnum>.GetWithError(RequestStatusEnum.NotAllowed);

        if (IsStatusDiposed(_currentStatus))
            return ResultBase<RequestStatusEnum>.GetWithError(RequestStatusEnum.Disposed);

        if (requestStatus.Equals(_currentStatus))
            return ResultBase<RequestStatusEnum>.GetSucess(RequestStatusEnum.AlreadyRequested);

        _currentStatus = requestStatus;

        return ResultBase<RequestStatusEnum>.GetSucess(RequestStatusEnum.Requested);
    }

    /// <summary>
    /// Set the <see cref="CurrentStatus"/> equals a Disposed with errors
    /// </summary>
    /// <remarks>
    ///     <para>Only acessible with the context thread</para>
    /// </remarks>
    /// <param name="e">Exception relationed with the disposed</param>
    /// <returns><see cref="ResultBase"/></returns>
    internal ResultBase<RequestStatusEnum> SetCurrentStatusWithException(Exception e)
    {
        var requestStatus = ContextRunEnum.DisposedWithError;

        if (Thread.CurrentThread.ManagedThreadId != _id)
            return ResultBase<RequestStatusEnum>.GetWithError(RequestStatusEnum.NotAllowed);

        if (IsStatusDiposed(_currentStatus))
            return ResultBase<RequestStatusEnum>.GetWithError(RequestStatusEnum.Disposed);

        if (requestStatus.Equals(_currentStatus))
            return ResultBase<RequestStatusEnum>.GetSucess(RequestStatusEnum.AlreadyRequested);

        _currentStatus = requestStatus;
        _exception = e;

        return ResultBase<RequestStatusEnum>.GetSucess(RequestStatusEnum.Requested);
    }

    /// <summary>
    /// Set the <see cref="CurrentStatus"/> equals a finished with sucess.
    /// </summary>
    /// <remarks>
    ///     <para>Only acessible with the context thread</para>
    /// </remarks>
    /// <returns><see cref="ResultBase"/></returns>
    internal ResultBase<RequestStatusEnum> SetCurrentStatusFinished()
    {
        var requestStatus = ContextRunEnum.DisposedBecauseIsFinished;

        if (Thread.CurrentThread.ManagedThreadId != _id)
            return ResultBase<RequestStatusEnum>.GetWithError(RequestStatusEnum.NotAllowed);

        if (IsStatusDiposed(_currentStatus))
            return ResultBase<RequestStatusEnum>.GetWithError(RequestStatusEnum.Disposed);

        if (requestStatus.Equals(_currentStatus))
            return ResultBase<RequestStatusEnum>.GetSucess(RequestStatusEnum.AlreadyRequested);

        _currentStatus = requestStatus;

        return ResultBase<RequestStatusEnum>.GetSucess(RequestStatusEnum.Requested);
    }

    /// <summary>
    /// check if status (<see cref="CurrentStatus"/> and <see cref="RequestStatus"/>) is the same.
    /// </summary>
    /// <returns>if true, status requested is the same of the current, if false, is not same</returns>
    public bool IsCurrentContextEqualsTheRequested()
    {
        if (_currentStatus.Equals(_requestStatus))
            return true;

        // verifying if is disposed
        if (IsStatusDiposed(_currentStatus) && IsStatusDiposed(_requestStatus))
            return true;

        return false;
    }

    /// <summary>
    /// Check if it is disposed
    /// </summary>
    /// <returns>true - it was disponsed, false - not was disposed</returns>
    public bool IsDisposed()
    {
        return IsStatusDiposed(_currentStatus);
    }

    private static bool IsStatusDiposed(ContextRunEnum status)
    {
        if (status.Equals(ContextRunEnum.Disposed) || 
            status.Equals(ContextRunEnum.DisposedWithError) ||
            status.Equals(ContextRunEnum.DisposedWithError))
            return true;

        return false;
    }
}
