using Dapper;
using TaskSchedulerScraping.Application.Dto.Scraping;
using TaskSchedulerScraping.Application.Repositories.Scraping;
using TaskSchedulerScraping.Domain.Entities.Scraping;
using TaskSchedulerScraping.Infrastructure.UoW;

namespace TaskSchedulerScraping.Infrastructure.Repositories.Scraping;

public class ScrapingExecuteRepository : RepositoryBase, IScrapingExecuteRepository
{
    public ScrapingExecuteRepository(IUnitOfWorkRepository unitOfWorkRepository) : base(unitOfWorkRepository)
    {
    }

    public async Task<ScrapingExecuteDto?> AddAsync(ScrapingExecute entity)
    {
        return
            await _connection.QueryFirstOrDefaultAsync<ScrapingExecuteDto?>(
                "INSERT INTO taskscheduler.scrapingexecute (IdScrapingModel, StartAt, EndAt) VALUES (@IdScrapingModel, @StartAt, @EndAt);" +
                "SELECT Id, IdScrapingModel, StartAt, EndAt FROM taskscheduler.scrapingexecute WHERE Id = last_insert_id();",
                entity,
                transaction: _transaction
            );
    }

    public async Task<ScrapingExecuteDto?> DeleteByIdAsync(int id)
    {
        var obj = await GetByIdOrDefaultAsync(id);

        if (obj is null)
            return null;

        await _connection.ExecuteAsync(
            "DELETE FROM taskscheduler.scrapingexecute WHERE Id = @Id;",
            new { id },
            transaction: _transaction
        );

        return obj;
    }

    public async Task<IEnumerable<ScrapingExecuteDto>> GetAllAsync()
    {
        return
            await _connection.QueryAsync<ScrapingExecuteDto>(
                "SELECT Id, IdScrapingModel, StartAt, EndAt FROM taskscheduler.scrapingexecute;",
                transaction: _transaction
            );
    }

    public async Task<IEnumerable<ScrapingExecuteDto>> GetAllByModelAsync(int idScrapingModel)
    {
        return
            await _connection.QueryAsync<ScrapingExecuteDto>(
                "SELECT Id, IdScrapingModel, StartAt, EndAt FROM taskscheduler.scrapingexecute WHERE IdScrapingModel=@IdScrapingModel;",
                new { idScrapingModel },
                transaction: _transaction
            );
    }

    public async Task<ScrapingExecuteDto?> GetByIdOrDefaultAsync(int id)
    {
        return
            await _connection.QueryFirstOrDefaultAsync<ScrapingExecuteDto?>(
                "SELECT Id, IdScrapingModel, StartAt, EndAt FROM taskscheduler.scrapingexecute WHERE Id = @Id;",
                new { id },
                transaction: _transaction
            );
    }

    public Task<ScrapingExecuteDto?> UpdateAsync(ScrapingExecute entity)
    {
        throw new NotImplementedException();
    }
}