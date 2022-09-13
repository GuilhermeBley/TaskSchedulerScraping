using TaskSchedulerScraping.Scraper.Model;
using TaskSchedulerScraping.Scraper.Tests.Mocks;
using TaskSchedulerScraping.Scraper.Tests.Monitors;

namespace TaskSchedulerScraping.Scraper.Tests;

public class ModelScraperTest
{
    [Fact]
    public void Test1()
    {
        var monitor = new SimpleMonitor();
        var exec = new SimpleExecution();
        var model = 
            new ModelScraper<SimpleExecution, SimpleData>
            (
                1,
                () => exec,
                () => SimpleDataFactory.GetData()
            )
            {
                WhenAllWorksEnd = (finishList) => { monitor.Resume(); }
            };

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        monitor.Wait(30*1000, ()=> Assert.True(false));

        Assert.True(exec.ExecHours.Any());

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess && model.State == ModelStateEnum.Disposed);
    }
}