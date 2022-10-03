using TaskSchedulerScraping.Scraper.DependencyInjection.Model;
using TaskSchedulerScraping.Scraper.Model;
using Microsoft.Extensions.DependencyInjection;

namespace TaskSchedulerScraping.Scraper.DependencyInjection.Tests;


public class ModelScraperDiTest
{
    [Fact(Timeout = 5000)]
    public async Task ExecuteModelDi_Services_TryExecutionWithOutConstructor()
    {
        var servicesBase 
            = new ServicesTestBase();
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


    [Fact(Timeout = 5000)]
    public async Task ExecuteModelDi_Services_TryExecutionWith100ThreadsWith1Constructor()
    {
        var servicesBase 
            = new ServicesTestBase((services) => {
                services.AddScoped<ISimpleService, SimpleService>();
            });
        var serviceProvider = servicesBase.ServiceProvider;
        bool hasErrorInExecution = false;

        IModelScraper model
            = new ModelScraperService<SimpleExecutionWithController, SimpleData>(
                100,
                serviceProvider,
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(100); },
                whenOccursException: (ex, data) => { hasErrorInExecution = true; return ExecutionResult.ThrowException(); }
            );
        
        await model.Run();

        await WaitModelFinish(model);

        Assert.False(hasErrorInExecution);
        Assert.Equal(ModelStateEnum.Disposed, model.State);
    }

    [Fact(Timeout = 5000)]
    public void ExecuteModelDi_Services_TryExecutionWith2Constructor()
    {
        var servicesBase 
            = new ServicesTestBase((services) => {
                services.AddScoped<ISimpleService, SimpleService>();
            });
        var serviceProvider = servicesBase.ServiceProvider;

        IModelScraper? model = null;

        Assert.ThrowsAny<Exception>(()=>{
            model
                = new ModelScraperService<SimpleExecutionWith2Controller, SimpleData>(
                    1,
                    serviceProvider,
                    async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(100); },
                    whenOccursException: (ex, data) => { return ExecutionResult.ThrowException(); }
                );
        });
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