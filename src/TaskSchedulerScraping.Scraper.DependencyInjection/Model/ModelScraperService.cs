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
    public ModelScraperService(
        int countScraper,
        IServiceProvider serviceProvider,
        Func<Task<IEnumerable<TData>>> getData
        ) : base(countScraper, GetContextWithServices(serviceProvider), getData)
    {
    }

    /// <summary>
    /// Instance with service provider
    /// </summary>
    /// <inheritdoc path="*"/>
    /// <param name="serviceProvider">Services</param>
    public ModelScraperService(
        int countScraper,
        IServiceProvider serviceProvider,
        Func<Task<IEnumerable<TData>>> getData,
        Func<Exception, TData, ExecutionResult>? whenOccursException = null,
        Action<ResultBase<TData>>? whenDataFinished = null,
        Action<IEnumerable<ResultBase<Exception?>>>? whenAllWorksEnd = null
        ) : base(countScraper, GetContextWithServices(serviceProvider), getData, whenOccursException, whenDataFinished, whenAllWorksEnd)
    {
    }

    /// <summary>
    /// Method make a function that returns a new instance of exection with our services
    /// </summary>
    /// <param name="serviceProvider">Services</param>
    /// <returns>Function that returns new </returns>
    /// <exception cref="Exception"></exception>
    private static Func<TExecutionContext> GetContextWithServices(IServiceProvider serviceProvider)
    {
        var executionType = typeof(TExecutionContext);

        if (executionType.GetConstructors().Where(c => c.IsPublic).Count() <= 1)
            throw new Exception($"{nameof(TExecutionContext)} should be have only one or zero public constructors.");

        var args = GetShraredArgs(serviceProvider).ToArray();

        var functionExc = () =>
        {
            return ActivatorUtilities.CreateInstance<TExecutionContext>(serviceProvider, args);
        };

        return functionExc;
    }

    private static object[] GetShraredArgs(IServiceProvider serviceProvider)
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

            try
            {
                sharedObj = Activator.CreateInstance(parameter.ParameterType);
            }
            catch { }

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

        return sharedArgs.ToArray();
    }
}
