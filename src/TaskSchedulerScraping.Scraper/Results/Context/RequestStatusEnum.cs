namespace TaskSchedulerScraping.Scraper.Results.Context;

internal enum RequestStatusEnum : sbyte
{
    Requested = 1,
    Disposed = 2,
    AlreadyRequested = 3,
    NotAllowed = 4
}