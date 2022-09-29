using TaskSchedulerScraping.Scraper.Model;
using TaskSchedulerScraping.Scraper.Results;

namespace TaskSchedulerScraping.Scraper.DependencyInjection.Model;
public class ModelScraperService<TExecutionContext, TData> : ModelScraper<TExecutionContext, TData>
    where TData : class
    where TExecutionContext : ExecutionContext<TData>
{
    public ModelScraperService(int countScraper, Func<TExecutionContext> getContext, Func<Task<IEnumerable<TData>>> getData) : base(countScraper, getContext, getData)
    {
    }

    public ModelScraperService(int countScraper, Func<TExecutionContext> getContext, Func<Task<IEnumerable<TData>>> getData, Func<Exception, TData, ExecutionResult>? whenOccursException = null, Action<ResultBase<TData>>? whenDataFinished = null, Action<IEnumerable<ResultBase<Exception?>>>? whenAllWorksEnd = null) : base(countScraper, getContext, getData, whenOccursException, whenDataFinished, whenAllWorksEnd)
    {
    }
}
