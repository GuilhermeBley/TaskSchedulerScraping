using TaskSchedulerScraping.Application.Dto.Scraping;
using TaskSchedulerScraping.Domain.Entities.Scraping;

namespace TaskSchedulerScraping.Application.Repositories.Scraping;

public interface IScrapingExecuteRepository : IRepositoryBase<ScrapingExecute, ScrapingExecuteDto, int>
{
    Task<IEnumerable<ScrapingExecuteDto>> GetAllByModelAsync(int idScrapingModel);
}