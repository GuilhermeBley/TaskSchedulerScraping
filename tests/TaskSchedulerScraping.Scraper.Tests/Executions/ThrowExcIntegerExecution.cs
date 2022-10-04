using TaskSchedulerScraping.Scraper.Model;
using TaskSchedulerScraping.Scraper.Results.Context;

namespace TaskSchedulerScraping.Scraper.Tests.Executions;

internal class ThrowExcIntegerExecution : Quest<IntegerData>
{
    private int _throwOnNumber { get; }
    private bool _hasThrow = true;
    public ContextRun Context { get; } = new ContextRun();
    public Action<IntegerData>? OnSearch;

    public ThrowExcIntegerExecution(int throwOnNumber)
    {
        _throwOnNumber = throwOnNumber;
    }

    public override void Dispose()
    {

    }

    public override QuestResult Execute(IntegerData data, CancellationToken cancellationToken = default)
    {
        if (_hasThrow && _throwOnNumber == data.Id)
        {
            _hasThrow = false;
            throw new Exception($"Throw on number {data.Id}.");
        }
            

        OnSearch?.Invoke(data);
        
        return QuestResult.Ok();
    }
}