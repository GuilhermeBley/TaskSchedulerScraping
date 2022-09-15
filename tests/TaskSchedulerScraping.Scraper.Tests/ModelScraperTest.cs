using System.Collections.Concurrent;
using TaskSchedulerScraping.Scraper.Model;
using TaskSchedulerScraping.Scraper.Tests.Mocks;
using TaskSchedulerScraping.Scraper.Tests.Monitors;

namespace TaskSchedulerScraping.Scraper.Tests;

public class ModelScraperTest
{
    [Fact]
    public void ExecuteModel_Exec_Add10ItemsToListWith1Thread()
    {
        BlockingCollection<DateTime> blockList = new();
        var monitor = new SimpleMonitor();
        IModelScraper model =
            new ModelScraper<SimpleExecution, SimpleData>
            (
                1,
                () => new SimpleExecution() { OnSearch = (timer) => { blockList.Add(timer); } },
                () => SimpleDataFactory.GetData(10)
            )
            {
                WhenAllWorksEnd = (finishList) => { monitor.Resume(); }
            };

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        monitor.Wait(30 * 1000, () => Assert.True(false));

        Assert.True(blockList.Count == 10);

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact]
    public void ExecuteModel_Exec_Add20ItemsToListWith2Thread()
    {
        BlockingCollection<DateTime> blockList = new();
        var monitor = new SimpleMonitor();
        IModelScraper model =
            new ModelScraper<SimpleExecution, SimpleData>
            (
                2,
                () => new SimpleExecution() { OnSearch = (timer) => { blockList.Add(timer); } },
                () => SimpleDataFactory.GetData(20)
            )
            {
                WhenAllWorksEnd = (finishList) => { monitor.Resume(); }
            };

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        monitor.Wait(30 * 1000, () => Assert.True(false));

        Assert.True(blockList.Count == 20);

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact]
    public void ExecuteModel_Exec_Add1000ItemsToListWith10Thread()
    {
        BlockingCollection<DateTime> blockList = new();
        var monitor = new SimpleMonitor();
        IModelScraper model =
            new ModelScraper<SimpleExecution, SimpleData>
            (
                10,
                () => new SimpleExecution() { OnSearch = (timer) => { blockList.Add(timer); } },
                () => SimpleDataFactory.GetData(1000)
            )
            {
                WhenAllWorksEnd = (finishList) => { monitor.Resume(); }
            };

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        monitor.Wait(30 * 1000, () => Assert.True(false));

        Assert.True(blockList.Count == 1000);

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact]
    public void ExecuteModel_Exec_Add1000ItemsToListWith10ThreadWithConfirmationData()
    {
        BlockingCollection<int> blockList = new();
        var monitor = new SimpleMonitor();
        IModelScraper model =
            new ModelScraper<SimpleExecution, SimpleData>
            (
                10,
                () => new SimpleExecution() { OnSearch = (timer) => { blockList.Add(Thread.CurrentThread.ManagedThreadId); } },
                () => SimpleDataFactory.GetData(1000)
            )
            {
                WhenAllWorksEnd = (finishList) => { monitor.Resume(); }
            };

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        monitor.Wait(30 * 1000, () => Assert.True(false));

        Assert.True(blockList.Distinct().Count() == 10);

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact]
    public void ExecuteModel_Oredered_ChecksOrderFromData()
    {
        BlockingCollection<int> blockList = new();
        var monitor = new SimpleMonitor();
        IModelScraper model =
            new ModelScraper<IntegerExecution, IntegerData>
            (
                1,
                () => new IntegerExecution() { OnSearch = (data) => { blockList.Add(data.Id); } },
                () => IntegerDataFactory.GetData(100)
            )
            {
                WhenAllWorksEnd = (finishList) => { monitor.Resume(); }
            };

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        monitor.Wait(30 * 1000, () => Assert.True(false));

        IReadOnlyList<int> readOnlyOrdered = blockList.ToList();
        for (int i = 0; i < blockList.Count; i++)
        {
            if (i == 0)
                continue;

            Assert.True(readOnlyOrdered[i-1] < readOnlyOrdered[i]);
        }

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact]
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
                () => IntegerDataFactory.GetData(100)
            )
            {
                WhenAllWorksEnd = (finishList) => { monitor.Resume(); }
            };

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        monitor.Wait(30 * 1000, () => Assert.True(false));

        Assert.Equal(onError, blockList.Last());

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact]
    public void PauseModel_Pause_Pause()
    {
        var monitor = new SimpleMonitor();
        IModelScraper model =
            new ModelScraper<EndlessExecution, SimpleData>
            (
                1,
                () => new EndlessExecution(),
                () => SimpleDataFactory.GetData(1)
            )
            {
                WhenAllWorksEnd = (finishList) => { monitor.Resume(); }
            };

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        var resultPause = model.PauseAsync();

        monitor.Wait(30 * 1000, () => Assert.True(false));

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }
}