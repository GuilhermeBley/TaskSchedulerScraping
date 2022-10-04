using TaskSchedulerScraping.Scraper.Model;
using TaskSchedulerScraping.Scraper.Results.Context;

namespace TaskSchedulerScraping.Scraper.Tests.Executions;

internal class EndlessWhileExecution : Quest<SimpleData>
{
    public bool InRepeat = true;

    public override void Dispose()
    {

    }

    public override QuestResult Execute(SimpleData data, CancellationToken cancellationToken = default)
    {
        while (InRepeat)
            Thread.Sleep(50);

        return QuestResult.Ok();
    }
}