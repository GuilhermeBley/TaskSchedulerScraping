namespace TaskSchedulerScraping.Scraper.Results.Models;

public enum RunModelEnum : sbyte
{
    FailedRequest = 0,
    OkRequest = 1,
    AlreadyExecuted = 2,
}

public class RunModel
{
    public PauseModelEnum Status { get; }
    public int CountRunWorkers { get; }
    public IEnumerable<string> Messages { get; }

    public RunModel(PauseModelEnum status, int countRunWorkers, params string[] messages)
    {
        Status = status;
        Messages = messages;
        CountRunWorkers = countRunWorkers;
    }

    public RunModel(PauseModelEnum status, int countRunWorkers, IEnumerable<string> messages)
    {
        Status = status;
        Messages = messages;
    }
}