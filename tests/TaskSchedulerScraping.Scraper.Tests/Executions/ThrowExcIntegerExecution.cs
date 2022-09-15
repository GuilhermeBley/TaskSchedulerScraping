using TaskSchedulerScraping.Scraper.Model;
using TaskSchedulerScraping.Scraper.Results.Context;

namespace TaskSchedulerScraping.Scraper.Tests.Executions;

internal class ThrowExcIntegerExecution : ExecutionContext<IntegerData>
{
    private int _throwOnNumber { get; }
    private bool _hasThrow = false;
    public ContextRun Context { get; } = new ContextRun();
    public Action<IntegerData>? OnSearch;

    public ThrowExcIntegerExecution(int throwOnNumber)
    {
        _throwOnNumber = throwOnNumber;
    }

    public override void Dispose()
    {

    }

    public override void Execute(IntegerData data)
    {
        if (!_hasThrow && _throwOnNumber == data.Id)
            throw new Exception($"Throw on number {data.Id}.");

        OnSearch?.Invoke(data);
    }
}