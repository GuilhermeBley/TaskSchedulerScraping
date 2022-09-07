namespace TaskSchedulerScraping.Scraper.Results.Models;

public class StopModel
{
    public StopModelEnum Status { get; }
    public string? Message { get; }

    public StopModel(StopModelEnum status, string? message = null)
    {
        Status = status;
        Message = message;
    }
}

public enum StopModelEnum : sbyte
{
    Stoped = 1,
    Failed = 2
}