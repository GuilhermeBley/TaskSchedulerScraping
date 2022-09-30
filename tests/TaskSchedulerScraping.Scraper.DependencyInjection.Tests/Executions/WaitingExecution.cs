using TaskSchedulerScraping.Scraper.Model;

namespace TaskSchedulerScraping.Scraper.DependencyInjection.Tests.Executions;

internal class WaitingExecution : ExecutionContext<IntegerData>
{
    private int _milisseconds { get; } = 1;

    public WaitingExecution(int milisseconds)
    {
        _milisseconds = milisseconds;
    }

    public override void Dispose()
    {
        
    }

    public override ExecutionResult Execute(IntegerData data, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Thread.Sleep(_milisseconds);

        return ExecutionResult.Ok();
    }
}