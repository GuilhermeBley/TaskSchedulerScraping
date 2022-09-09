namespace TaskSchedulerScraping.Scraper.Results.Models;

public enum RunModelEnum : sbyte
{
    FailedRequest = 0,
    OkRequest = 1,
    AlreadyExecuted = 2,
    Disposed = 3
}

public class RunModel
{
    public RunModelEnum Status { get; }
    public int CountRunWorkers { get; }
    public IEnumerable<string> Messages { get; }

    public RunModel(RunModelEnum status, int countRunWorkers, params string[] messages)
    {
        Status = status;
        Messages = messages;
        CountRunWorkers = countRunWorkers;
    }

    public RunModel(RunModelEnum status, int countRunWorkers, IEnumerable<string> messages)
    {
        Status = status;
        Messages = messages;
    }
}