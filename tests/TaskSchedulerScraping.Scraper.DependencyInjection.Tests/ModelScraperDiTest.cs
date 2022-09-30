using TaskSchedulerScraping.Scraper.DependencyInjection.Model;
using TaskSchedulerScraping.Scraper.Model;
using Microsoft.Extensions.DependencyInjection;

namespace TaskSchedulerScraping.Scraper.DependencyInjection.Tests;


public class ModelScraperDiTest
{
    [Fact(Timeout = 5000)]
    public async Task ExecuteModelDi_Services_TryWithService()
    {
        var servicesBase 
            = new ServicesTestBase((services) => {
                services.AddScoped<ISimpleService, SimpleService>();
            });
        var serviceProvider = servicesBase.ServiceProvider;

        IModelScraper model 
            = new ModelScraperService<SimpleExecution, SimpleData>(
                1,
                serviceProvider,
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(1); }
            );

        await model.Run();

        await WaitModelFinish(model);

        Assert.Equal(ModelStateEnum.Disposed, model.State);
    }

    public static async Task WaitModelFinish(IModelScraper model, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        while(model.State != ModelStateEnum.Disposed)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(400);
        }
    }
}