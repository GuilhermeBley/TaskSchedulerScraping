using TaskSchedulerScraping.Scraper.Model;
using TaskSchedulerScraping.Scraper.Tests.Mocks;

namespace TaskSchedulerScraping.Scraper.Tests;

public class ModelScraperTest
{
    [Fact]
    public void Test1()
    {
        var exec = new SimpleExecution();
        var model = 
            new ModelScraper<SimpleExecution, SimpleData>
            (
                1,
                () => exec,
                () => SimpleDataFactory.GetData()
            );

        var resultRun = model.Run();

        Assert.True(resultRun.IsSucess);

        DateTime start = DateTime.Now;
        while (true)
        {
            
        }

        Assert.True(exec.ExecHours.Any());

        var resultStop = model.StopAsync().GetAwaiter().GetResult();

        Assert.True(resultStop.IsSucess);
    }
}