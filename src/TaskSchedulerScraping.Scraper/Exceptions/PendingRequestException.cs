namespace TaskSchedulerScraping.Scraper.Exceptions;

/// <summary>
/// Use when a pending request should be attended, and is necessary go back.
/// </summary>
public class PendingRequestException : Exception
{
    /// <summary>
    /// Message pattern
    /// </summary>
    private const string ConstMessage = "Has pending request to check.";

    /// <summary>
    /// Instance of <see cref="PendingRequestException"/>
    /// </summary>
    public PendingRequestException() : base(ConstMessage)
    {
    }
}