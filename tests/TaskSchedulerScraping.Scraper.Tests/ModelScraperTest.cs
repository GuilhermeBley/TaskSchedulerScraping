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
        var model = 
            new ModelScraper<SimpleExecution, SimpleData>
            (
                1,
                () => new SimpleExecution(){ OnSearch = (timer) => { blockList.Add(timer); } },
                () => SimpleDataFactory.GetData(10)
            )
            {
                WhenAllWorksEnd = (finishList) => { monitor.Resume(); }
            };

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        monitor.Wait(30*1000, ()=> Assert.True(false));

        Assert.True(blockList.Count==10);

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact]
    public void ExecuteModel_Exec_Add20ItemsToListWith2Thread()
    {
        BlockingCollection<DateTime> blockList = new();
        var monitor = new SimpleMonitor();
        var model = 
            new ModelScraper<SimpleExecution, SimpleData>
            (
                2,
                () => new SimpleExecution(){ OnSearch = (timer) => { blockList.Add(timer); } },
                () => SimpleDataFactory.GetData(20)
            )
            {
                WhenAllWorksEnd = (finishList) => { monitor.Resume(); }
            };

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        monitor.Wait(30*1000, ()=> Assert.True(false));

        Assert.True(blockList.Count==20);

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact]
    public void ExecuteModel_Exec_Add1000ItemsToListWith10Thread()
    {
        BlockingCollection<DateTime> blockList = new();
        var monitor = new SimpleMonitor();
        var model = 
            new ModelScraper<SimpleExecution, SimpleData>
            (
                10,
                () => new SimpleExecution(){ OnSearch = (timer) => { blockList.Add(timer); } },
                () => SimpleDataFactory.GetData(1000)
            )
            {
                WhenAllWorksEnd = (finishList) => { monitor.Resume(); }
            };

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        monitor.Wait(30*1000, ()=> Assert.True(false));

        Assert.True(blockList.Count==1000);

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }

    [Fact]
    public void ExecuteModel_Exec_Add1000ItemsToListWith10ThreadWithConfirmationData()
    {
        BlockingCollection<int> blockList = new();
        var monitor = new SimpleMonitor();
        var model = 
            new ModelScraper<SimpleExecution, SimpleData>
            (
                10,
                () => new SimpleExecution(){ OnSearch = (timer) => { blockList.Add(Thread.CurrentThread.ManagedThreadId); } },
                () => SimpleDataFactory.GetData(1000)
            )
            {
                WhenAllWorksEnd = (finishList) => { monitor.Resume(); }
            };

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        monitor.Wait(30*1000, ()=> Assert.True(false));

        Assert.True(blockList.Distinct().Count()==10);

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }
}