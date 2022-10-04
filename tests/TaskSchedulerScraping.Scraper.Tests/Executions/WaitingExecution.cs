using TaskSchedulerScraping.Scraper.Model;

namespace TaskSchedulerScraping.Scraper.Tests.Executions;

internal class WaitingExecution : Quest<IntegerData>
{
    private int _milisseconds { get; } = 1;

    public WaitingExecution(int milisseconds)
    {
        _milisseconds = milisseconds;
    }

    public override void Dispose()
    {
        
    }

    public override QuestResult Execute(IntegerData data, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Thread.Sleep(_milisseconds);

        return QuestResult.Ok();
    }
}