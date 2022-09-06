using System.Collections.Concurrent;
using TaskSchedulerScraping.Scraper.Results.Models;
using TaskSchedulerScraping.Scraper.Results.Context;
using TaskSchedulerScraping.Scraper.Results;

namespace TaskSchedulerScraping.Scraper.Model;

public abstract class ModelScraper : IModelScraper, IDisposable
{
    private ConcurrentDictionary<int,
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task<ResultBase<PauseModel>> PauseAsync(bool pause = true)
    {
        throw new NotImplementedException();
    }

    public Task<ResultBase<RunModel>> RunAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ResultBase<StopModel>> StopAsync()
    {
        throw new NotImplementedException();
    }
}

public class ModelScraper<T> : ModelScraper
{

}
