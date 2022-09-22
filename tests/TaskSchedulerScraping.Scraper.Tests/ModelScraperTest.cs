using System.Collections.Concurrent;
using TaskSchedulerScraping.Scraper.Model;
using TaskSchedulerScraping.Scraper.Tests.Mocks;
using TaskSchedulerScraping.Scraper.Tests.Monitors;

namespace TaskSchedulerScraping.Scraper.Tests;

public class ModelScraperTest
{
    [Fact(Timeout = 5000)]
    public void ExecuteModel_Exec_Add10ItemsToListWith1Thread()
    {
        BlockingCollection<DateTime> blockList = new();
        var monitor = new SimpleMonitor();
        IModelScraper model =
            new ModelScraper<SimpleExecution, SimpleData>
            (
                1,
                () => new SimpleExecution() { OnSearch = (timer) => { blockList.Add(timer); } },
                () => SimpleDataFactory.GetData(10),
                whenAllWorksEnd: (finishList) => { monitor.Resume(); }
            );

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        monitor.Wait(30 * 1000, () => Assert.True(false));

        Assert.True(blockList.Count == 10);

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact(Timeout = 5000)]
    public void ExecuteModel_Exec_Add20ItemsToListWith2Thread()
    {
        BlockingCollection<DateTime> blockList = new();
        var monitor = new SimpleMonitor();
        IModelScraper model =
            new ModelScraper<SimpleExecution, SimpleData>
            (
                2,
                () => new SimpleExecution() { OnSearch = (timer) => { blockList.Add(timer); } },
                () => SimpleDataFactory.GetData(20),
                whenAllWorksEnd: (finishList) => { monitor.Resume(); }
            );

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        monitor.Wait(30 * 1000, () => Assert.True(false));

        Assert.True(blockList.Count == 20);

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact(Timeout = 5000)]
    public void ExecuteModel_Exec_Add1000ItemsToListWith10Thread()
    {
        BlockingCollection<DateTime> blockList = new();
        var monitor = new SimpleMonitor();
        IModelScraper model =
            new ModelScraper<SimpleExecution, SimpleData>
            (
                10,
                () => new SimpleExecution() { OnSearch = (timer) => { blockList.Add(timer); } },
                () => SimpleDataFactory.GetData(1000),
                whenAllWorksEnd: (finishList) => { monitor.Resume(); }
            );

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        monitor.Wait(30 * 1000, () => Assert.True(false));

        Assert.True(blockList.Count == 1000);

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact(Timeout = 5000)]
    public void ExecuteModel_Exec_Add1000ItemsToListWith10ThreadWithConfirmationData()
    {
        BlockingCollection<int> blockList = new();
        var monitor = new SimpleMonitor();
        IModelScraper model =
            new ModelScraper<SimpleExecution, SimpleData>
            (
                10,
                () => new SimpleExecution() { OnSearch = (timer) => { blockList.Add(Thread.CurrentThread.ManagedThreadId); } },
                () => SimpleDataFactory.GetData(1000),
                whenAllWorksEnd: (finishList) => { monitor.Resume(); }
            );

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        monitor.Wait(30 * 1000, () => Assert.True(false));

        Assert.True(blockList.Distinct().Count() == 10);

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact(Timeout = 3000)]
    public void ExecuteModel_Oredered_ChecksOrderFromData()
    {
        BlockingCollection<int> blockList = new();
        var monitor = new SimpleMonitor();
        IModelScraper model =
            new ModelScraper<IntegerExecution, IntegerData>
            (
                1,
                () => new IntegerExecution() { OnSearch = (data) => { blockList.Add(data.Id); } },
                () => IntegerDataFactory.GetData(100),
                whenAllWorksEnd: (finishList) => { monitor.Resume(); }
            );

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        monitor.Wait(2 * 1000, () => Assert.True(false));

        IReadOnlyList<int> readOnlyOrdered = blockList.ToList();
        for (int i = 0; i < blockList.Count; i++)
        {
            if (i == 0)
                continue;

            Assert.True(readOnlyOrdered[i - 1] < readOnlyOrdered[i]);
        }

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact(Timeout = 2000)]
    public void ExecuteModel_Oredered_ChecksOrderFromDataWithException()
    {
        const int onError = 32;
        BlockingCollection<int> blockList = new();
        var monitor = new SimpleMonitor();
        IModelScraper model =
            new ModelScraper<ThrowExcIntegerExecution, IntegerData>
            (
                1,
                () => new ThrowExcIntegerExecution(onError) { OnSearch = (data) => { blockList.Add(data.Id); } },
                () => IntegerDataFactory.GetData(100),
                whenAllWorksEnd: (finishList) => { monitor.Resume(); },
                whenOccursException: (exception, data) => { return ExecutionResult.RetryOther(); }
            );

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        monitor.Wait(1 * 1000, () => Assert.True(false));

        Assert.Equal(onError, blockList.Last());

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact(Timeout = 2000)]
    public async Task PauseModel_Pause_Pause()
    {
        bool hasError = false;
        var monitor = new SimpleMonitor();
        IModelScraper model =
            new ModelScraper<EndlessExecution, SimpleData>
            (
                1,
                () => new EndlessExecution() { OnRepeat = (containsError) => { hasError = containsError; } },
                () => SimpleDataFactory.GetData(1),
                whenAllWorksEnd: (finishList) => { monitor.Resume(); }
            );

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        var resultPause = model.PauseAsync();

        var pause = await model.PauseAsync(true);

        Assert.True(pause.IsSucess);
        
        monitor.Wait(1 * 1000, ()=>model.PauseAsync(false).GetAwaiter().GetResult());

        var resultStop = await model.StopAsync();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed && hasError);
    }

    [Fact(Timeout = 2000)]
    public async Task StopModel_Dispose_Stop()
    {
        var monitor = new SimpleMonitor();
        IModelScraper model =
            new ModelScraper<EndlessExecution, SimpleData>
            (
                1,
                () => new EndlessExecution(),
                () => SimpleDataFactory.GetData(1),
                whenAllWorksEnd: (finishList) => { monitor.Resume(); }
            );

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        var cancellationTokenSource = new CancellationTokenSource();

        new Thread(() => { Thread.Sleep(1000); cancellationTokenSource.Cancel(); });

        var resultStop = await model.StopAsync(cancellationTokenSource.Token);

        Assert.True(resultStop.IsSucess);

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact(Timeout = 2000)]
    public async Task PauseModel_Pause_CancelPause()
    {
        var monitor = new SimpleMonitor();
        IModelScraper model =
            new ModelScraper<EndlessWhileExecution, SimpleData>
            (
                1,
                () => new EndlessWhileExecution(),
                () => SimpleDataFactory.GetData(1),
                whenAllWorksEnd: (finishList) => { monitor.Resume(); }
            );

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        var cancellationTokenSource = new CancellationTokenSource();

        new Thread(
            () => { Thread.Sleep(50); cancellationTokenSource.Cancel(); }
        ).Start();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            ()=>model.PauseAsync(true, cancellationToken: cancellationTokenSource.Token));
    }

    [Fact(Timeout = 2000)]
    public async Task StopModel_Dispose_CancelStop()
    {
        var monitor = new SimpleMonitor();
        IModelScraper model =
            new ModelScraper<EndlessWhileExecution, SimpleData>
            (
                1,
                () => new EndlessWhileExecution(),
                () => SimpleDataFactory.GetData(1),
                whenAllWorksEnd: (finishList) => { monitor.Resume(); }
            );

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        var cancellationTokenSource = new CancellationTokenSource();

        new Thread(() => { Thread.Sleep(50); cancellationTokenSource.Cancel(); }).Start();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            ()=>model.StopAsync(cancellationToken: cancellationTokenSource.Token));
    }
}