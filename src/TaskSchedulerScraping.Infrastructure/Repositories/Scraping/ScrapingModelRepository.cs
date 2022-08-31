using Dapper;
using TaskSchedulerScraping.Application.Dto.Scraping;
using TaskSchedulerScraping.Application.Repositories.Scraping;
using TaskSchedulerScraping.Domain.Entities.Scraping;
using TaskSchedulerScraping.Infrastructure.UoW;

namespace TaskSchedulerScraping.Infrastructure.Repositories.Scraping;

public class ScrapingModelRepository : RepositoryBase, IScrapingModelRepository
{
    public ScrapingModelRepository(IUnitOfWorkRepository unitOfWorkRepository) : base(unitOfWorkRepository)
    {
    }

    public async Task<ScrapingModelDto?> AddAsync(ScrapingModel entity)
    {
        return
            await _connection.QueryFirstOrDefaultAsync<ScrapingModelDto?>(
                "INSERT INTO taskscheduler.scrapingmodel (Name, NormalizedName, Description) VALUES (@Name, @NormalizedName, @Description);" +
                "SELECT Id, Name, NormalizedName, Description FROM taskscheduler.scrapingmodel WHERE Id = last_insert_id();",
                entity,
                transaction: _transaction
            );
    }

    public async Task<ScrapingModelDto?> DeleteByIdAsync(int id)
    {
        var obj = await GetByIdOrDefaultAsync(id);

        if (obj is null)
            return null;

        await _connection.ExecuteAsync(
            "DELETE FROM taskscheduler.scrapingmodel WHERE Id = @Id;",
            new { id },
            transaction: _transaction
        );

        return obj;
    }

    public async Task<IEnumerable<ScrapingModelDto>> GetAllAsync()
    {
        return
            await _connection.QueryAsync<ScrapingModelDto>(
                "SELECT Id, Name, NormalizedName, Description FROM taskscheduler.scrapingmodel;",
                transaction: _transaction
            );
    }

    public async Task<ScrapingModelDto?> GetByIdOrDefaultAsync(int id)
    {
        return
            await _connection.QueryFirstOrDefaultAsync<ScrapingModelDto>(
                "SELECT Id, Name, NormalizedName, Description FROM taskscheduler.scrapingmodel WHERE Id=@Id;",
                new { id },
                transaction: _transaction
            );
    }

    public async Task<ScrapingModelDto?> GetByNameAsync(string normalizedName)
    {
        return
            await _connection.QueryFirstOrDefaultAsync<ScrapingModelDto>(
                "SELECT Id, Name, NormalizedName, Description FROM taskscheduler.scrapingmodel WHERE NormalizedName=@NormalizedName;",
                new { normalizedName },
                transaction: _transaction
            );
    }

    public Task<ScrapingModelDto?> UpdateAsync(ScrapingModel entity)
    {
        throw new NotImplementedException();
    }
}