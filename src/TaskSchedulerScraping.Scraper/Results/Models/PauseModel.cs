namespace TaskSchedulerScraping.Scraper.Results.Models;

public enum PauseModelEnum : sbyte
{
    Paused = 0,
    Running = 1,
    Failed = 2,
    InProcess = 3
}

public class PauseModel
{
    public PauseModelEnum Status { get; }
    public string? Message { get; }

    public PauseModel(PauseModelEnum status, string? message = null)
    {
        Status = status;
        Message = message;
    }
}