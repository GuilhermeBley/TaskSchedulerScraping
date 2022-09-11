using TaskSchedulerScraping.Scraper.Model;
using TaskSchedulerScraping.Scraper.Results.Context;

namespace TaskSchedulerScraping.Scraper.Tests.Executions;

internal class SimpleExecution : IExecutionContext<SimpleData>
{
    public int Id => throw new NotImplementedException();

    public ContextRun Context => throw new NotImplementedException();

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void Execute(SimpleData data)
    {
        throw new NotImplementedException();
    }

    public void Pause(bool pause = true)
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }
}