using TaskSchedulerScraping.Scraper.DependencyInjection.Model;
using TaskSchedulerScraping.Scraper.Model;
using Microsoft.Extensions.DependencyInjection;

namespace TaskSchedulerScraping.Scraper.DependencyInjection.Tests;

//
// Tests sintaxe: MethodName_ExpectedBehavior_StateUnderTest
// Example: isAdult_AgeLessThan18_False
//


public class ModelScraperDiTest
{
    [Fact(Timeout = 5000)]
    public async Task ModelServices_InstanceAndRunWithoutServices_Sucess()
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
    public async Task ModelServices_TryExecutionWith100ThreadsWith1ConstructorInExc_Sucess()
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
    public void ModelServices_TryExecutionWith100ThreadsWith2ConstructorInExc_Failed()
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
                    100,
                    serviceProvider,
                    async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(100); },
                    whenOccursException: (ex, data) => { return ExecutionResult.ThrowException(); }
                );
        });
    }

    [Fact(Timeout = 5000)]
    public async Task ModelServices_DiWithObjAndService_Sucess()
    {
        var servicesBase 
            = new ServicesTestBase((services) => {
                services.AddScoped<ISimpleService, SimpleService>();
            });
        var serviceProvider = servicesBase.ServiceProvider;

        IModelScraper? model = null;
        
        model
            = new ModelScraperService<SimpleExecutionServiceAndObj, SimpleData>(
                1,
                serviceProvider,
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(100); },
                whenOccursException: (ex, data) => { return ExecutionResult.ThrowException(); },
                args : new Obj1()
            );

        await RunAndWaitAsync(model);
    }

    [Fact(Timeout = 5000)]
    public async Task ModelServices_DiWithoutObjAndWithService_FailedRunExpectInvalidOperation()
    {
        var servicesBase
            = new ServicesTestBase((services) =>
            {
                services.AddScoped<ISimpleService, SimpleService>();
            });
        var serviceProvider = servicesBase.ServiceProvider;

        IModelScraper? model = null;

        IEnumerable<Results.ResultBase<Exception?>> excList = Enumerable.Empty<Results.ResultBase<Exception?>>();
        model
            = new ModelScraperService<SimpleExecutionServiceAndObj, SimpleData>(
                100,
                serviceProvider,
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(100); },
                whenOccursException: (ex, data) => { return ExecutionResult.ThrowException(); },
                whenAllWorksEnd: (list) => { excList = list; }
            );

        await RunAndWaitAsync(model);

        Assert.All(excList, result => Assert.IsType<InvalidOperationException>(result.Result));
    }

    [Fact(Timeout = 5000)]
    public async Task ModelServices_DiWithoutService_FailedRunExpectInvalidOperation()
    {
        var servicesBase
            = new ServicesTestBase((services) =>
            {
            });
        var serviceProvider = servicesBase.ServiceProvider;

        IModelScraper? model = null;

        var serviceArgs = new SimpleService();
        IEnumerable<Results.ResultBase<Exception?>> excList = Enumerable.Empty<Results.ResultBase<Exception?>>();
        model
            = new ModelScraperService<SimpleExecutionServiceAndObj, SimpleData>(
                100,
                serviceProvider,
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(100); },
                whenOccursException: (ex, data) => { return ExecutionResult.ThrowException(); },
                whenAllWorksEnd: (list) => { excList = list; },
                args: serviceArgs
            );

        await RunAndWaitAsync(model);

        Assert.All(excList, result => Assert.IsType<InvalidOperationException>(result.Result));
    }

    [Fact(Timeout = 5000)]
    public async Task ModelServices_DiWithoutServiceAndObj_FailedRunExpectInvalidOperation()
    {
        var servicesBase
            = new ServicesTestBase((services) =>
            {
            });
        var serviceProvider = servicesBase.ServiceProvider;

        IModelScraper? model = null;

        IEnumerable<Results.ResultBase<Exception?>> excList = Enumerable.Empty<Results.ResultBase<Exception?>>();
        model
            = new ModelScraperService<SimpleExecutionServiceAndObj, SimpleData>(
                100,
                serviceProvider,
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(100); },
                whenOccursException: (ex, data) => { return ExecutionResult.ThrowException(); },
                whenAllWorksEnd: (list) => { excList = list; }
            );

        await RunAndWaitAsync(model);

        Assert.All(excList, result => Assert.IsType<InvalidOperationException>(result.Result));
    }

    [Fact(Timeout = 5000)]
    public async Task ModelServices_DiWithIServiceInArgs_SucessArgsPriority()
    {
        var servicesBase
            = new ServicesTestBase((services) =>
            {
                services.AddScoped<ISimpleService, SimpleService>();
            });
        var serviceProvider = servicesBase.ServiceProvider;

        IModelScraper? model = null;
        ISimpleService service = new SimpleService();
        List<ISimpleService> servicesOnCreate = new();
        IEnumerable<Results.ResultBase<Exception?>> excList = Enumerable.Empty<Results.ResultBase<Exception?>>();
        model
            = new ModelScraperService<SimpleExecutionWithController, SimpleData>(
                100,
                serviceProvider,
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(100); },
                whenExecutionCreated: (context) => servicesOnCreate.Add(context.Service),
                whenAllWorksEnd: (list) => { excList = list; },
                args: service
            );

        await RunAndWaitAsync(model);

        Assert.All(excList, result => Assert.True(result.IsSucess));

        Assert.All(servicesOnCreate, result => Assert.Equal(service, result));
    }

    [Fact(Timeout = 5000)]
    public async Task ModelServices_DiWithServiceInArgs_SucessArgsPriority()
    {
        var servicesBase
            = new ServicesTestBase((services) =>
            {
                services.AddScoped<ISimpleService, SimpleService>();
            });
        var serviceProvider = servicesBase.ServiceProvider;

        IModelScraper? model = null;
        SimpleService service = new SimpleService();
        List<ISimpleService> servicesOnCreate = new();
        IEnumerable<Results.ResultBase<Exception?>> excList = Enumerable.Empty<Results.ResultBase<Exception?>>();
        model
            = new ModelScraperService<SimpleExecutionWithController, SimpleData>(
                100,
                serviceProvider,
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(100); },
                whenExecutionCreated: (context) => servicesOnCreate.Add(context.Service),
                whenAllWorksEnd: (list) => { excList = list; },
                args: service
            );

        await RunAndWaitAsync(model);

        Assert.All(excList, result => Assert.True(result.IsSucess));

        Assert.All(servicesOnCreate, result => Assert.Equal(service, result));
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


    /// <summary>
    /// Runs model and expected sucess in execute
    /// </summary>
    /// <remarks>
    ///     <para>Wait async with cancellation</para>
    /// </remarks>
    /// <param name="model">model to execute</param>
    /// <param name="cancellationToken">token to cancel wait</param>
    public static async Task RunAndWaitAsync(IModelScraper model, CancellationToken cancellationToken = default)
    {
        var result = await model.Run();

        Assert.True(result.IsSucess, "Failed to run model.");

        await WaitModelFinish(model, cancellationToken);

        Assert.Equal(ModelStateEnum.Disposed, model.State);
    }
}