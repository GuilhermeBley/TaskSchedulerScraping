using TaskSchedulerScraping.Scraper.Model;
using TaskSchedulerScraping.Scraper.Results.Context;

namespace TaskSchedulerScraping.Scraper.Tests.Executions;

internal class EndlessExecution : ExecutionContext<SimpleData>
{
    public ContextRun Context { get; } = new ContextRun();
    public bool _hasError = false;
    public Action? OnRepeat;

    public override void Dispose()
    {

    }

    public override ExecutionResult Execute(SimpleData data)
    {
        while (true)
        {
            if (_hasError)
                return ExecutionResult.Ok();

            Thread.Sleep(50);
            OnRepeat?.Invoke();
            
            try
            {
                CheckState();
            }
            catch
            {
                _hasError = true;
                throw;
            }
            
        }
    }
}