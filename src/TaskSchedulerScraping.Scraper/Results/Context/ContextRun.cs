namespace TaskSchedulerScraping.Scraper.Results.Context;

public class ContextRun
{
    public ContextRunEnum CurrentStatus { get; internal set; }
    public ContextRunEnum RequestStatus { get; internal set; }

    internal ResultBase<RequestStatusEnum> SetRequestStatus(ContextRunEnum requestStatus)
    {
        if (RequestStatus.Equals(ContextRunEnum.Disposed) || CurrentStatus.Equals(ContextRunEnum.Disposed))
            return ResultBase<RequestStatusEnum>.GetWithError(RequestStatusEnum.Dispoded);
    }
}
