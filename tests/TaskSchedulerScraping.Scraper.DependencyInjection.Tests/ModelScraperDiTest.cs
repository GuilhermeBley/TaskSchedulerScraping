using TaskSchedulerScraping.Scraper.DependencyInjection.Model;
using TaskSchedulerScraping.Scraper.Model;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

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
    public async Task ModelServices_InstanceAndRunWithObjUnusedInExc_Sucess()
    {
        var servicesBase 
            = new ServicesTestBase();
        var serviceProvider = servicesBase.ServiceProvider;

        IModelScraper model 
            = new ModelScraperService<SimpleExecution, SimpleData>(
                1,
                serviceProvider,
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(1); },
                args : new object[]{ new Obj1(), new Obj2() }
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
                whenOccursException: (ex, data) => { hasErrorInExecution = true; return QuestResult.ThrowException(); }
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
                    whenOccursException: (ex, data) => { return QuestResult.ThrowException(); }
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
                whenOccursException: (ex, data) => { return QuestResult.ThrowException(); },
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
                whenOccursException: (ex, data) => { return QuestResult.ThrowException(); },
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
                whenOccursException: (ex, data) => { return QuestResult.ThrowException(); },
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
                whenOccursException: (ex, data) => { return QuestResult.ThrowException(); },
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

    [Fact(Timeout = 5000)]
    public async Task ServiceProvidier_DiWithTransient_FailedAllDifferent()
    {
        var servicesBase
            = new ServicesTestBase((services) =>
            {
                services.AddTransient<ISimpleService, SimpleService>();
                services.AddScoped<IServiceLinkedWithOther, ServiceLinkedWithOther>();
            });
        var serviceProvider = servicesBase.ServiceProvider;

        IModelScraper? model = null;
        
        List<(ISimpleService SimpleService, IServiceLinkedWithOther LinkedWithOther)> servicesOnCreate = new();
        IEnumerable<Results.ResultBase<Exception?>> excList = Enumerable.Empty<Results.ResultBase<Exception?>>();
        model
            = new ModelScraperService<SimpleExecutionLifeTimeService, SimpleData>(
                100,
                serviceProvider,
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(100); },
                whenExecutionCreated: (context) => servicesOnCreate.Add((context.Service, context.LinkedService)),
                whenAllWorksEnd: (list) => { excList = list; }
            );

        await RunAndWaitAsync(model);

        Assert.All(excList, result => Assert.True(result.IsSucess));

        Assert.All(servicesOnCreate, result 
            => Assert.NotEqual(result.SimpleService, result.LinkedWithOther.SimpleService));
    }

    [Fact(Timeout = 5000)]
    public async Task ServiceProvidier_DiWithScooped_SucessAllExcEqual()
    {
        var servicesBase
            = new ServicesTestBase((services) =>
            {
                services.AddScoped<ISimpleService, SimpleService>();
                services.AddScoped<IServiceLinkedWithOther, ServiceLinkedWithOther>();
            });
        var serviceProvider = servicesBase.ServiceProvider;

        IModelScraper? model = null;
        
        List<(ISimpleService SimpleService, IServiceLinkedWithOther LinkedWithOther)> servicesOnCreate = new();
        IEnumerable<Results.ResultBase<Exception?>> excList = Enumerable.Empty<Results.ResultBase<Exception?>>();
        model
            = new ModelScraperService<SimpleExecutionLifeTimeService, SimpleData>(
                100,
                serviceProvider,
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(100); },
                whenExecutionCreated: (context) => servicesOnCreate.Add((context.Service, context.LinkedService)),
                whenAllWorksEnd: (list) => { excList = list; }
            );

        await RunAndWaitAsync(model);

        Assert.All(excList, result => Assert.True(result.IsSucess));

        Assert.All(servicesOnCreate, result 
            => Assert.Equal(result.SimpleService, result.LinkedWithOther.SimpleService));

        var firstService = servicesOnCreate.First().SimpleService;

        Assert.NotEqual(firstService, servicesOnCreate.Last().SimpleService);
    }

    [Fact(Timeout = 5000)]
    public async Task ServiceProvidier_DiWithSingleton_SucessAllEqual()
    {
        var servicesBase
            = new ServicesTestBase((services) =>
            {
                services.AddSingleton<ISimpleService, SimpleService>();
                services.AddScoped<IServiceLinkedWithOther, ServiceLinkedWithOther>();
            });
        var serviceProvider = servicesBase.ServiceProvider;

        var simpleServiceSingledon = serviceProvider.GetService<ISimpleService>();

        IModelScraper? model = null;
        
        List<(ISimpleService SimpleService, IServiceLinkedWithOther LinkedWithOther)> servicesOnCreate = new();
        IEnumerable<Results.ResultBase<Exception?>> excList = Enumerable.Empty<Results.ResultBase<Exception?>>();
        model
            = new ModelScraperService<SimpleExecutionLifeTimeService, SimpleData>(
                100,
                serviceProvider,
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(100); },
                whenExecutionCreated: (context) => servicesOnCreate.Add((context.Service, context.LinkedService)),
                whenAllWorksEnd: (list) => { excList = list; }
            );

        await RunAndWaitAsync(model);

        Assert.All(excList, result => Assert.True(result.IsSucess));

        Assert.All(servicesOnCreate, result 
            => Assert.Equal(simpleServiceSingledon, result.SimpleService));

        Assert.All(servicesOnCreate, result 
            => Assert.Equal(simpleServiceSingledon, result.LinkedWithOther.SimpleService));
    }

    [Fact(Timeout = 5000)]
    public async Task ShareService_DiSameServiceWithTransient_Sucess()
    {
        var servicesBase
            = new ServicesTestBase((services) =>
            {
                services.AddTransient<ISimpleService, SimpleService>();
            });
        var serviceProvider = servicesBase.ServiceProvider;

        BlockingCollection<ISimpleService> servicesInExecutionsModel1 = new();
        IEnumerable<Results.ResultBase<Exception?>> excList1 = Enumerable.Empty<Results.ResultBase<Exception?>>();
        var model1
            = new ModelScraperService<SimpleExecutionShareService, SimpleData>(
                100,
                serviceProvider,
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(100); },
                whenExecutionCreated: (context) => servicesInExecutionsModel1.Add(context.SharedService),
                whenAllWorksEnd: (list) => { excList1 = list; }
            );

        await RunAndWaitAsync(model1);

        Assert.All(excList1, result => Assert.True(result.IsSucess));

        var serviceModel1 = servicesInExecutionsModel1.FirstOrDefault() 
            ?? throw new ArgumentNullException(nameof(servicesInExecutionsModel1));

        Assert.All(servicesInExecutionsModel1, result 
            => Assert.Equal(serviceModel1, result));

        BlockingCollection<ISimpleService> servicesInExecutionsModel2 = new();
        IEnumerable<Results.ResultBase<Exception?>> excList2 = Enumerable.Empty<Results.ResultBase<Exception?>>();
        var model2
            = new ModelScraperService<SimpleExecutionShareService, SimpleData>(
                100,
                serviceProvider,
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(100); },
                whenExecutionCreated: (context) => servicesInExecutionsModel2.Add(context.SharedService),
                whenAllWorksEnd: (list) => { excList1 = list; }
            );

        await RunAndWaitAsync(model2);

        Assert.All(excList2, result => Assert.True(result.IsSucess));

        Assert.All(servicesInExecutionsModel2, result 
            => Assert.NotEqual(serviceModel1, result));
    }

    [Fact(Timeout = 5000)]
    public async Task ShareService_DiSameServiceWithScooped_Sucess()
    {
        var servicesBase
            = new ServicesTestBase((services) =>
            {
                services.AddScoped<ISimpleService, SimpleService>();
            });
        var serviceProvider = servicesBase.ServiceProvider;

        BlockingCollection<ISimpleService> servicesInExecutionsModel1 = new();
        IEnumerable<Results.ResultBase<Exception?>> excList1 = Enumerable.Empty<Results.ResultBase<Exception?>>();
        var model1
            = new ModelScraperService<SimpleExecutionShareService, SimpleData>(
                100,
                serviceProvider,
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(100); },
                whenExecutionCreated: (context) => servicesInExecutionsModel1.Add(context.SharedService),
                whenAllWorksEnd: (list) => { excList1 = list; }
            );

        await RunAndWaitAsync(model1);

        Assert.All(excList1, result => Assert.True(result.IsSucess));

        var serviceModel1 = servicesInExecutionsModel1.FirstOrDefault() 
            ?? throw new ArgumentNullException(nameof(servicesInExecutionsModel1));

        Assert.All(servicesInExecutionsModel1, result 
            => Assert.Equal(serviceModel1, result));

        BlockingCollection<ISimpleService> servicesInExecutionsModel2 = new();
        IEnumerable<Results.ResultBase<Exception?>> excList2 = Enumerable.Empty<Results.ResultBase<Exception?>>();
        var model2
            = new ModelScraperService<SimpleExecutionShareService, SimpleData>(
                100,
                serviceProvider,
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(100); },
                whenExecutionCreated: (context) => servicesInExecutionsModel2.Add(context.SharedService),
                whenAllWorksEnd: (list) => { excList1 = list; }
            );

        await RunAndWaitAsync(model2);

        Assert.All(excList2, result => Assert.True(result.IsSucess));

        Assert.All(servicesInExecutionsModel2, result 
            => Assert.NotEqual(serviceModel1, result));
    }
    
    [Fact(Timeout = 5000)]
    public async Task ShareService_DiSameServiceWithSingleton_Sucess()
    {
        var servicesBase
            = new ServicesTestBase((services) =>
            {
                services.AddSingleton<ISimpleService, SimpleService>();
            });
        var serviceProvider = servicesBase.ServiceProvider;

        var singledonService = serviceProvider.GetRequiredService<ISimpleService>();

        BlockingCollection<ISimpleService> servicesInExecutionsModel1 = new();
        IEnumerable<Results.ResultBase<Exception?>> excList1 = Enumerable.Empty<Results.ResultBase<Exception?>>();
        var model1
            = new ModelScraperService<SimpleExecutionShareService, SimpleData>(
                100,
                serviceProvider,
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(100); },
                whenExecutionCreated: (context) => servicesInExecutionsModel1.Add(context.SharedService),
                whenAllWorksEnd: (list) => { excList1 = list; }
            );

        await RunAndWaitAsync(model1);

        Assert.All(excList1, result => Assert.True(result.IsSucess));

        Assert.All(servicesInExecutionsModel1, result 
            => Assert.Equal(singledonService, result));

        BlockingCollection<ISimpleService> servicesInExecutionsModel2 = new();
        IEnumerable<Results.ResultBase<Exception?>> excList2 = Enumerable.Empty<Results.ResultBase<Exception?>>();
        var model2
            = new ModelScraperService<SimpleExecutionShareService, SimpleData>(
                100,
                serviceProvider,
                async () => { await Task.CompletedTask; return SimpleDataFactory.GetData(100); },
                whenExecutionCreated: (context) => servicesInExecutionsModel2.Add(context.SharedService),
                whenAllWorksEnd: (list) => { excList1 = list; }
            );

        await RunAndWaitAsync(model2);

        Assert.All(excList2, result => Assert.True(result.IsSucess));

        Assert.All(servicesInExecutionsModel2, result 
            => Assert.Equal(singledonService, result));
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