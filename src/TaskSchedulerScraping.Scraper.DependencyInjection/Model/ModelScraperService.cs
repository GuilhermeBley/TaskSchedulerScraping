using Microsoft.Extensions.DependencyInjection;
using TaskSchedulerScraping.Scraper.Model;
using TaskSchedulerScraping.Scraper.Results;
using TaskSchedulerScraping.Scraper.DependencyInjection.Attributes;

namespace TaskSchedulerScraping.Scraper.DependencyInjection.Model;

/// <summary>
/// Model scaper which manages services to executions
/// </summary>
/// <inheritdoc path="*"/>
public class ModelScraperService<TExecutionContext, TData> : ModelScraper<TExecutionContext, TData>
    where TData : class
    where TExecutionContext : ExecutionContext<TData>
{
    /// <summary>
    /// Instance with service provider
    /// </summary>
    /// <param name="serviceProvider">Services</param>
    /// <param name="args">Same obj to constructors of executions</param>
    public ModelScraperService(
        int countScraper,
        IServiceProvider serviceProvider,
        Func<Task<IEnumerable<TData>>> getData,
        params object[] args) : base(countScraper, GetContextWithServices(serviceProvider, null, args), getData)
    {
    }

    /// <summary>
    /// Instance with service provider
    /// </summary>
    /// <inheritdoc path="*"/>
    /// <param name="serviceProvider">Services</param>
    /// <param name="args">Same obj to constructors of executions</param>
    /// <param name="whenExecutionCreated">When execution was created, this action was invoked with new instance of <typeparamref name="TExecutionContext"/></param>
    public ModelScraperService(
        int countScraper,
        IServiceProvider serviceProvider,
        Func<Task<IEnumerable<TData>>> getData,
        Func<Exception, TData, ExecutionResult>? whenOccursException = null,
        Action<ResultBase<TData>>? whenDataFinished = null,
        Action<IEnumerable<ResultBase<Exception?>>>? whenAllWorksEnd = null,
        Action<IEnumerable<TData>>? whenDataWasCollected = null,
        Action<TExecutionContext>? whenExecutionCreated = null,
        params object[] args
        ) : base(countScraper, GetContextWithServices(serviceProvider, whenExecutionCreated, args), getData, whenOccursException, whenDataFinished, whenAllWorksEnd, whenDataWasCollected)
    {
    }

    /// <summary>
    /// Method make a function that returns a new instance of exection with our services
    /// </summary>
    /// <param name="serviceProvider">Services</param>
    /// <returns>Function that returns new <typeparamref name="TExecutionContext"/></returns>
    /// <exception cref="Exception"></exception>
    private static Func<TExecutionContext> GetContextWithServices(IServiceProvider serviceProvider, Action<TExecutionContext>? whenExecutionCreated = null, params object[] args)
    {
        var executionType = typeof(TExecutionContext);

        if (executionType.GetConstructors().Where(c => c.IsPublic).Count() > 1)
            throw new Exception($"{nameof(TExecutionContext)} should be have only one or zero public constructors.");

        args = Enumerable.Concat(args ,GetShraredArgs(serviceProvider, args)).ToArray();
        
        var functionExc = () =>
        {
            var exc = ActivatorUtilities.CreateInstance<TExecutionContext>(serviceProvider, args);
            whenExecutionCreated?.Invoke(exc);
            return exc;
        };

        return functionExc;
    }

    /// <summary>
    /// Method get shared objects which the parameters is tagged with <see cref="ScraperObjSharedAttribute"/>
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    private static IEnumerable<object> GetShraredArgs(IServiceProvider serviceProvider, params object[] args)
    {
        var executionType = typeof(TExecutionContext);

        List<object> sharedArgs = new();

        var constructor = executionType.GetConstructors().Where(c => c.IsPublic).FirstOrDefault();

        if (constructor is null)
            return sharedArgs.ToArray();

        foreach (var parameter in constructor.GetParameters())
        {
            if (!parameter.GetCustomAttributes(typeof(ScraperObjSharedAttribute), true).Any())
                continue;

            object? sharedObj = null;

            sharedObj = args.FirstOrDefault(o => o.GetType().Equals(parameter.ParameterType));

            if (sharedObj != null)
            {
                sharedArgs.Add(sharedObj);
                continue;
            }

            sharedObj = serviceProvider.GetService(parameter.ParameterType);

            if (sharedObj != null)
            {
                sharedArgs.Add(sharedObj);
                continue;
            }

            throw new ArgumentException(
                $"Parameter of constructor '{nameof(TExecutionContext)}' didn't be found or create.", 
                parameter.ParameterType.ToString());

        }

        return sharedArgs;
    }
}
