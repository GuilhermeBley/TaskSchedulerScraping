using TaskSchedulerScraping.Application.Dto.Scraping;
using TaskSchedulerScraping.Domain.Entities.Scraping;

namespace TaskSchedulerScraping.Application.Repositories.Scraping;

public interface IScrapingModelRepository : IRepositoryBase<ScrapingModel, ScrapingModelDto, int>
{
    Task<ScrapingModelDto?> GetByNameAsync(string normalizedName);
}