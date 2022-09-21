using TaskSchedulerScraping.Scraper.Model;
using TaskSchedulerScraping.Scraper.Results.Context;

namespace TaskSchedulerScraping.Scraper.Tests.Executions;

internal class EndlessExecution : ExecutionContext<SimpleData>
{
    public bool IsActiveError = true;
    public bool _hasError = false;
    public Action<bool>? OnRepeat;

    public override void Dispose()
    {

    }

    public override ExecutionResult Execute(SimpleData data)
    {
        while (true)
        {

            OnRepeat?.Invoke(_hasError);

            if (_hasError)
                return ExecutionResult.Ok();

            Thread.Sleep(50);
            
            try
            {
                CheckState();
            }
            catch
            {
                if (IsActiveError)
                    _hasError = true;
                throw;
            }
            
        }
    }
}