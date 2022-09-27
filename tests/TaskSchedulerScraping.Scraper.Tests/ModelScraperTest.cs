using System.Collections.Concurrent;
using TaskSchedulerScraping.Scraper.Model;
using TaskSchedulerScraping.Scraper.Tests.Mocks;
using TaskSchedulerScraping.Scraper.Tests.Monitors;

namespace TaskSchedulerScraping.Scraper.Tests;

public class ModelScraperTest
{
    [Fact(Timeout = 5000)]
    public async Task ExecuteModel_Exec_Add10ItemsToListWith1Thread()
    {
        BlockingCollection<DateTime> blockList = new();
        var isFinished = false;
        IModelScraper model =
            new ModelScraper<SimpleExecution, SimpleData>
            (
                1,
                () => new SimpleExecution() { OnSearch = (timer) => { blockList.Add(timer); } },
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(10); },
                whenAllWorksEnd: (finishList) => { isFinished = true; }
            );

        var resultRun = await model.Run();

        Assert.True(resultRun.IsSucess);

        await WaitFinishModel(model);

        Assert.True(isFinished);

        Assert.True(blockList.Count == 10);

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact(Timeout = 5000)]
    public async Task ExecuteModel_Exec_Add20ItemsToListWith2Thread()
    {
        BlockingCollection<DateTime> blockList = new();
        var isFinished = false;
        IModelScraper model =
            new ModelScraper<SimpleExecution, SimpleData>
            (
                2,
                () => new SimpleExecution() { OnSearch = (timer) => { blockList.Add(timer); } },
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(20); },
                whenAllWorksEnd: (finishList) => { isFinished = true; }
            );

        var resultRun = await model.Run();

        Assert.True(resultRun.IsSucess);

        await WaitFinishModel(model);

        Assert.True(blockList.Count == 20 && isFinished);

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact(Timeout = 5000)]
    public async Task ExecuteModel_Exec_Add1000ItemsToListWith10Thread()
    {
        BlockingCollection<DateTime> blockList = new();
        var isFinished = false;
        IModelScraper model =
            new ModelScraper<SimpleExecution, SimpleData>
            (
                10,
                () => new SimpleExecution() { OnSearch = (timer) => { blockList.Add(timer); } },
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(1000); },
                whenAllWorksEnd: (finishList) => { isFinished = true; }
            );

        var resultRun = await model.Run();

        Assert.True(resultRun.IsSucess);

        await WaitFinishModel(model);

        Assert.True(blockList.Count == 1000 && isFinished);

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact(Timeout = 5000)]
    public async Task ExecuteModel_Exec_Add1000ItemsToListWith10ThreadWithConfirmationData()
    {
        BlockingCollection<int> blockList = new();
        var isFinished = false;
        IModelScraper model =
            new ModelScraper<SimpleExecution, SimpleData>
            (
                10,
                () => new SimpleExecution() { OnSearch = (timer) => { blockList.Add(Thread.CurrentThread.ManagedThreadId); } },
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(1000); },
                whenAllWorksEnd: (finishList) => { isFinished = true; }
            );

        var resultRun = await model.Run();

        Assert.True(resultRun.IsSucess);

        await WaitFinishModel(model);

        Assert.True(isFinished);

        Assert.True(blockList.Distinct().Count() == 10);

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact(Timeout = 3000)]
    public async Task ExecuteModel_Oredered_ChecksOrderFromData()
    {
        BlockingCollection<int> blockList = new();
        var isFinished = false;
        IModelScraper model =
            new ModelScraper<IntegerExecution, IntegerData>
            (
                1,
                () => new IntegerExecution() { OnSearch = (data) => { blockList.Add(data.Id); } },
                async () => { await Task.CompletedTask; return IntegerDataFactory.GetData(100); },
                whenAllWorksEnd: (finishList) => { isFinished = true; }
            );

        var resultRun = await model.Run();

        Assert.True(resultRun.IsSucess);

        await WaitFinishModel(model);

        IReadOnlyList<int> readOnlyOrdered = blockList.ToList();
        for (int i = 0; i < blockList.Count; i++)
        {
            if (i == 0)
                continue;

            Assert.True(readOnlyOrdered[i - 1] < readOnlyOrdered[i]);
        }

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed && isFinished);
    }

    [Fact(Timeout = 2000)]
    public async Task ExecuteModel_Oredered_ChecksOrderFromDataWithException()
    {
        const int onError = 32;
        BlockingCollection<int> blockList = new();
        var isFinished = false;
        IModelScraper model =
            new ModelScraper<ThrowExcIntegerExecution, IntegerData>
            (
                1,
                () => new ThrowExcIntegerExecution(onError) { OnSearch = (data) => { blockList.Add(data.Id); } },
                async () => { await Task.CompletedTask; return IntegerDataFactory.GetData(100); },
                whenAllWorksEnd: (finishList) => { isFinished = true; },
                whenOccursException: (exception, data) => { return ExecutionResult.RetryOther(); }
            );

        var resultRun = await model.Run();

        Assert.True(resultRun.IsSucess);

        await WaitFinishModel(model);

        Assert.True(isFinished);

        Assert.Equal(onError, blockList.Last());

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact(Timeout = 2000)]
    public async Task PauseModel_Pause_Pause()
    {
        bool hasError = false;
        var monitor = new SimpleMonitor();
        var isFinished = false;
        IModelScraper model =
            new ModelScraper<EndlessExecution, SimpleData>
            (
                1,
                () => new EndlessExecution() { OnRepeat = (containsError) => { hasError = containsError; } },
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(1); },
                whenAllWorksEnd: (finishList) => { isFinished = true; }
            );

        var resultRun = await model.Run();

        Assert.True(resultRun.IsSucess);

        var pause = await model.PauseAsync(true);

        Assert.True(pause.IsSucess);

        monitor.Wait(1000, () => model.PauseAsync(false).GetAwaiter().GetResult());

        await WaitFinishModel(model);

        Assert.True(isFinished);
        Assert.True(hasError);
        Assert.True(model.State == ModelStateEnum.Disposed);
    }

    [Fact(Timeout = 2000)]
    public async Task StopModel_Dispose_Stop()
    {
        var monitor = new SimpleMonitor();
        var isFinished = false;
        IModelScraper model =
            new ModelScraper<EndlessExecution, SimpleData>
            (
                1,
                () => new EndlessExecution(),
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(1); },
                whenAllWorksEnd: (finishList) => { isFinished = true; }
            );

        var resultRun = await model.Run();

        Assert.True(resultRun.IsSucess);

        var cancellationTokenSource = new CancellationTokenSource();

        new Thread(() => { Thread.Sleep(1000); cancellationTokenSource.Cancel(); });

        var resultStop = await model.StopAsync(cancellationTokenSource.Token);

        Assert.True(resultStop.IsSucess);

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed && isFinished);
    }

    [Fact(Timeout = 2000)]
    public async Task PauseModel_Pause_CancelPause()
    {
        IModelScraper model =
            new ModelScraper<EndlessWhileExecution, SimpleData>
            (
                1,
                () => new EndlessWhileExecution(),
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(1); }
            );

        var resultRun = await model.Run();

        Assert.True(resultRun.IsSucess);

        var cancellationTokenSource = new CancellationTokenSource();

        new Thread(
            () => { Thread.Sleep(50); cancellationTokenSource.Cancel(); }
        ).Start();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => model.PauseAsync(true, cancellationToken: cancellationTokenSource.Token));
    }

    [Fact(Timeout = 2000)]
    public async Task StopModel_Dispose_CancelStop()
    {
        IModelScraper model =
            new ModelScraper<EndlessWhileExecution, SimpleData>
            (
                1,
                () => new EndlessWhileExecution(),
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(1); }
            );

        var resultRun = await model.Run();

        Assert.True(resultRun.IsSucess);

        var cancellationTokenSource = new CancellationTokenSource();

        new Thread(() => { Thread.Sleep(50); cancellationTokenSource.Cancel(); }).Start();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => model.StopAsync(cancellationToken: cancellationTokenSource.Token));
    }

    [Fact(Timeout = 1000)]
    public async Task ExecuteModel_Exec_With0DataAnd1ThreadWithoutError()
    {
        BlockingCollection<DateTime> blockList = new();
        var isFinished = false;
        var monitor = new SimpleMonitor();
        IModelScraper model =
            new ModelScraper<SimpleExecution, SimpleData>
            (
                1,
                () => new SimpleExecution() { OnSearch = (timer) => { blockList.Add(timer); } },
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(0); },
                whenAllWorksEnd: (finishList) => { isFinished = true; monitor.Resume(); Assert.True(finishList.All(f => f.IsSucess)); },
                whenDataFinished: (result) => { Assert.True(false); }
            );

        var resultRun = await model.Run();

        Assert.True(resultRun.IsSucess);

        if (!isFinished)
            monitor.Wait(1 * 1000, () => Assert.True(isFinished));

        Assert.True(!blockList.Any());

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact(Timeout = 1000)]
    public async Task ExecuteModel_Exec_With0DataAnd10ThreadWithoutError()
    {
        BlockingCollection<DateTime> blockList = new();
        var isFinished = false;
        var monitor = new SimpleMonitor();
        IModelScraper model =
            new ModelScraper<SimpleExecution, SimpleData>
            (
                10,
                () => new SimpleExecution() { OnSearch = (timer) => { blockList.Add(timer); } },
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(0); },
                whenAllWorksEnd: (finishList) => { isFinished = true; monitor.Resume(); Assert.True(finishList.All(f => f.IsSucess)); },
                whenDataFinished: (result) => { Assert.True(false); }
            );

        var resultRun = await model.Run();

        Assert.True(resultRun.IsSucess);

        if (!isFinished)
            monitor.Wait(1 * 1000);

        Assert.True(!blockList.Any());

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact(Timeout = 1000)]
    public async Task ExecuteModel_Exec_With0DataAnd100ThreadWithoutError()
    {
        BlockingCollection<DateTime> blockList = new();
        var monitor = new SimpleMonitor();
        IModelScraper model =
            new ModelScraper<SimpleExecution, SimpleData>
            (
                100,
                () => new SimpleExecution() { OnSearch = (timer) => { blockList.Add(timer); } },
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(0); },
                whenAllWorksEnd: (finishList) => { monitor.Resume(); Assert.True(finishList.All(f => f.IsSucess)); },
                whenDataFinished: (result) => { Assert.True(false); }
            );

        var resultRun = await model.Run();

        Assert.True(resultRun.IsSucess);

        Assert.True(!blockList.Any());

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact(Timeout = 1000)]
    public async Task ExecuteModel_Exec_WhenAllWorkFinished()
    {
        BlockingCollection<DateTime> blockList = new();
        bool isWhenAllWorksFinished = false;
        IModelScraper model =
            new ModelScraper<SimpleExecution, SimpleData>
            (
                50,
                () => new SimpleExecution() { OnSearch = (timer) => { blockList.Add(timer); } },
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(0); },
                whenAllWorksEnd: (finishList) => { isWhenAllWorksFinished = true; }
            );

        var resultRun = await model.Run();

        Assert.True(resultRun.IsSucess);

        Assert.True(!blockList.Any());

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(isWhenAllWorksFinished);
        Assert.True(model.State == ModelStateEnum.Disposed);
        Assert.True(resultStop.IsSucess);
    }

    [Fact(Timeout = 1000)]
    public async Task ExecuteModel_Exec_WhenDataFinished()
    {
        BlockingCollection<DateTime> blockList = new();
        bool isWhenDataFinished = false;
        IModelScraper model =
            new ModelScraper<SimpleExecution, SimpleData>
            (
                50,
                () => new SimpleExecution() { OnSearch = (timer) => { blockList.Add(timer); } },
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(1); },
                whenDataFinished: (data) => { isWhenDataFinished = true; }
            );

        var resultRun = await model.Run();

        Assert.True(resultRun.IsSucess);

        await WaitFinishModel(model);

        Assert.True(blockList.Any());

        Assert.True(model.State == ModelStateEnum.Disposed && isWhenDataFinished);
    }

    [Fact(Timeout = 1000)]
    public async Task ExecuteModel_Exec_WhenOccursExceptionOk()
    {
        BlockingCollection<int> blockList = new();
        bool isWhenDataFinished = false;
        bool isSucessDataSearch = false;
        IModelScraper model =
            new ModelScraper<ThrowExcIntegerExecution, IntegerData>
            (
                50,
                () => new ThrowExcIntegerExecution(1) { OnSearch = (data) => { blockList.Add(data.Id); } },
                async () => { await Task.CompletedTask; return IntegerDataFactory.GetData(1); },
                whenDataFinished: (data) => { isSucessDataSearch = data.IsSucess; isWhenDataFinished = true; },
                whenOccursException: (ex, data) => { return ExecutionResult.Ok(); }
            );

        var resultRun = await model.Run();

        Assert.True(resultRun.IsSucess);

        Assert.True(!blockList.Any());

        await WaitFinishModel(model);

        Assert.True(model.State == ModelStateEnum.Disposed &&
            isWhenDataFinished && isSucessDataSearch);
    }

    [Fact(Timeout = 1000)]
    public async Task ExecuteModel_Exec_WhenPauseDoesNotCollectData()
    {
        const int maxWaiting = 100;
        const int maxData = 100;

        bool isPaused = false;
        bool collectDataInPause = false;
        BlockingCollection<int> blockList = new();

        var monitor = new SimpleMonitor();

        IModelScraper model =
            new ModelScraper<WaitingExecution, IntegerData>
            (
                maxData / 2,
                () => new WaitingExecution(maxWaiting),
                async () => { await Task.CompletedTask; return IntegerDataFactory.GetData(maxData); },
                whenDataFinished: (data) =>
                {
                    if (data.IsSucess)
                        blockList.Add(data.Result.Id);

                    if (isPaused)
                        collectDataInPause = true;
                }
            );

        var resultRun = await model.Run();

        Assert.True(resultRun.IsSucess);

        var resultPause = await model.PauseAsync();

        Assert.True(resultPause.IsSucess);

        monitor.Wait(maxWaiting * 3);

        Assert.True(!collectDataInPause);

        isPaused = false;

        resultPause = await model.PauseAsync(false);

        await WaitFinishModel(model);

        Assert.True(blockList.Count == maxData);

        Assert.True(model.State == ModelStateEnum.Disposed);
    }

    [Fact(Timeout = 1000)]
    public async Task ExecuteModel_Exec_WhenStopDoesNotCollectData()
    {
        const int maxWaiting = 100;
        const int maxData = 100;

        BlockingCollection<int> blockList = new();

        var monitor = new SimpleMonitor();

        IModelScraper model =
            new ModelScraper<WaitingExecution, IntegerData>
            (
                maxData / 2,
                () => new WaitingExecution(maxWaiting),
                async () => { await Task.CompletedTask; return IntegerDataFactory.GetData(maxData); },
                whenDataFinished: (data) =>
                {
                    if (data.IsSucess)
                        blockList.Add(data.Result.Id);
                }
            );

        var resultRun = await model.Run();

        Assert.True(resultRun.IsSucess);

        monitor.Wait(maxWaiting / 10);

        var resultStop = await model.StopAsync();

        Assert.True(resultStop.IsSucess);

        await WaitFinishModel(model);

        Assert.True(blockList.Count < maxData);

        Assert.True(model.State == ModelStateEnum.Disposed);
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