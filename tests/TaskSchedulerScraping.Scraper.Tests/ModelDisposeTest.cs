using System.Collections.Concurrent;
using TaskSchedulerScraping.Scraper.Model;
using TaskSchedulerScraping.Scraper.Tests.Mocks;
using TaskSchedulerScraping.Scraper.Tests.Monitors;
using Xunit.Abstractions;

namespace TaskSchedulerScraping.Scraper.Tests;

//
// Tests sintaxe: MethodName_ExpectedBehavior_StateUnderTest
// Example: isAdult_AgeLessThan18_False
//

public class ModelDisposeTest
{
    private readonly ITestOutputHelper _output;

    public ModelDisposeTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact(Timeout = 5000)]
    public async Task Dispose_RunAndDispose_SucessDisposeOrWaitDipose()
    {
        IModelScraper model =
            new ModelScraper<IntegerExecution, IntegerData>
            (
                100,
                () => new IntegerExecution(),
                async () => { await Task.CompletedTask; return IntegerDataFactory.GetData(100); }
            );

        using (model)
        {
            Assert.True((await model.Run()).IsSucess);
        }

        Assert.True(ModelStateEnum.Disposed == model.State || 
            ModelStateEnum.WaitingDispose == model.State);
    }

    [Fact(Timeout = 5000)]
    public async Task Dispose_RunAndDisposeAsync_SucessDispose()
    {
        IModelScraper model =
            new ModelScraper<IntegerExecution, IntegerData>
            (
                100,
                () => new IntegerExecution(),
                async () => { await Task.CompletedTask; return IntegerDataFactory.GetData(100); }
            );

        using (model)
        {
            Assert.True((await model.Run()).IsSucess);
        }

        Assert.Equal(ModelStateEnum.Disposed, model.State);
    }

    [Fact(Timeout = 5000)]
    public async Task Dispose_DisposeAndRun_FailedRun()
    {
        bool searched = false;
        IModelScraper model =
            new ModelScraper<IntegerExecution, IntegerData>
            (
                100,
                () => new IntegerExecution(),
                async () => { await Task.CompletedTask; return IntegerDataFactory.GetData(100); },
                whenDataFinished: (dataResult) => searched = true
            );

        using (model)
        {
            
        }

        var result = await model.Run();

        Assert.False(searched);
        Assert.False(result.IsSucess);
        Assert.Equal(ModelStateEnum.Disposed, model.State);
    }

    [Fact(Timeout = 5000)]
    public async Task Dispose_DisposeAndRunPauseAndUnpause_SucessDisposeOrWait()
    {
        IModelScraper model =
            new ModelScraper<IntegerExecution, IntegerData>
            (
                100,
                () => new IntegerExecution(),
                async () => { await Task.CompletedTask; return IntegerDataFactory.GetData(100); }
            );

        using (model)
        {
            Assert.True((await model.Run()).IsSucess);    
        }

        var resultRun = await model.Run();
        Assert.False(resultRun.IsSucess);

        var resultPause = await model.PauseAsync(true);
        Assert.False(resultPause.IsSucess);

        var resultUnPause = await model.PauseAsync(true);
        Assert.False(resultUnPause.IsSucess);

        Assert.True(ModelStateEnum.Disposed == model.State || 
            ModelStateEnum.WaitingDispose == model.State);
    }

    /// <summary>
    /// Wait to finish the model
    /// </summary>
    /// <returns>async</returns>
    /// <exception cref="OperationCanceledException"/>
    public async Task WaitFinishModel(IModelScraper model, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        while (model.State != ModelStateEnum.Disposed)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(250);
        }

        return;
    }
}