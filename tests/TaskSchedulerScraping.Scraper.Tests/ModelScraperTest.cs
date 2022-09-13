using TaskSchedulerScraping.Scraper.Model;
using TaskSchedulerScraping.Scraper.Tests.Mocks;

namespace TaskSchedulerScraping.Scraper.Tests;

public class ModelScraperTest
{
    [Fact]
    public void Test1()
    {
        var model = 
            new ModelScraper<SimpleExecution, SimpleData>
            (
                1,
                () => new SimpleExecution(),
                () => SimpleDataFactory.GetData()
            );

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        Thread.Sleep(200);

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess);
    }
}