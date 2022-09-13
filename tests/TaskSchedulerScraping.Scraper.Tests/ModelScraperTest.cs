using System.Collections.Concurrent;
using TaskSchedulerScraping.Scraper.Model;
using TaskSchedulerScraping.Scraper.Tests.Mocks;
using TaskSchedulerScraping.Scraper.Tests.Monitors;

namespace TaskSchedulerScraping.Scraper.Tests;

public class ModelScraperTest
{
    [Fact]
    public void Test1()
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
}