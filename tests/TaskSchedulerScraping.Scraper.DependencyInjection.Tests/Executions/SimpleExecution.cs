using TaskSchedulerScraping.Scraper.Model;

namespace TaskSchedulerScraping.Scraper.DependencyInjection.Tests.Executions;

internal class SimpleExecution : Quest<SimpleData>
{
    public override void Dispose()
    {
        
    }

    public override QuestResult Execute(SimpleData data, CancellationToken cancellationToken = default)
    {
        Thread.Sleep(10);
        return QuestResult.Ok();
    }
}