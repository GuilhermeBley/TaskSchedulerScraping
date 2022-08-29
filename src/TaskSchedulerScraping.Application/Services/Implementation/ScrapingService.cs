using TaskSchedulerScraping.Application.Dto.Scraping;
using TaskSchedulerScraping.Application.Repositories.Scraping;
using TaskSchedulerScraping.Application.Services.Interfaces;
using TaskSchedulerScraping.Application.UoW;

namespace TaskSchedulerScraping.Application.Services.Implementation;

public sealed class ScrapingService : IScrapingService
{
    private readonly IScrapingExecuteRepository _scrapingExecuteRepository;
    private readonly IScrapingModelRepository _scrapingModelRepository;
    private readonly IUnitOfWork _uoW;

    public ScrapingService(
        IScrapingExecuteRepository scrapingExecuteRepository,
        IScrapingModelRepository scrapingModelRepository,
        IUnitOfWork uoW)
    {
        _scrapingExecuteRepository = scrapingExecuteRepository;
        _scrapingModelRepository = scrapingModelRepository;
        _uoW = uoW;
    }

    public Task<ScrapingExecuteDto> AddScrapingExecuteAsync(ScrapingExecuteDto scrapingExecute)
    {
        throw new NotImplementedException();
    }

    public Task<ScrapingModelDto> AddScrapingModelAsync(ScrapingModelDto scrapingModel)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ScrapingExecuteDto>> GetAllScrapingExecuteAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ScrapingExecuteDto>> GetAllScrapingExecuteByModelAsync(int idScrapingModel)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ScrapingModelDto>> GetAllScrapingModelAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ScrapingModelDto> GetScrapingModelByNameAsync(string normalizedName)
    {
        throw new NotImplementedException();
    }
}