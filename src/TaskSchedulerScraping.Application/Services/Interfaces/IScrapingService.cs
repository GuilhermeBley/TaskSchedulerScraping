using TaskSchedulerScraping.Application.Dto.Scraping;

namespace TaskSchedulerScraping.Application.Services.Interfaces;

public interface IScrapingService
{
    Task<ScrapingExecuteDto> AddScrapingExecuteAsync(ScrapingExecuteDto scrapingExecute);
    Task<ScrapingModelDto> AddScrapingModelAsync(ScrapingModelDto scrapingModel);

    Task<IEnumerable<ScrapingExecuteDto>> GetAllScrapingExecuteAsync();
    Task<IEnumerable<ScrapingExecuteDto>> GetAllScrapingExecuteByModelAsync(int idScrapingModel);
    Task<IEnumerable<ScrapingModelDto>> GetAllScrapingModelAsync();
    Task<ScrapingModelDto> GetScrapingModelByNameAsync(string normalizedName);
    
}