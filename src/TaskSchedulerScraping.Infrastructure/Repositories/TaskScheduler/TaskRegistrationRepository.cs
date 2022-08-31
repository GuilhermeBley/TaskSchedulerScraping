using Dapper;
using TaskSchedulerScraping.Application.Dto.TaskScheduler;
using TaskSchedulerScraping.Application.Repositories.TaskScheduler;
using TaskSchedulerScraping.Domain.Entities.TaskScheduler;
using TaskSchedulerScraping.Infrastructure.UoW;

namespace TaskSchedulerScraping.Infrastructure.Repositories.TaskScheduler;

public class TaskRegistrationRepository : RepositoryBase, ITaskRegistrationRepository
{
    public TaskRegistrationRepository(IUnitOfWorkRepository unitOfWorkRepository) : base(unitOfWorkRepository)
    {
    }

    public async Task<TaskRegistrationDto?> AddAsync(TaskRegistration entity)
    {
        return
            await _connection.QueryFirstOrDefaultAsync<TaskRegistrationDto?>(
                "INSERT INTO taskscheduler.taskregistration (IdTaskGroup, Name, NormalizedName, Location, Description, Author) VALUES (@IdTaskGroup, @Name, @NormalizedName, @Location, @Description, @Author);" +
                "SELECT Id, IdTaskGroup, Name, NormalizedName, Location, Description, Author FROM taskscheduler.taskregistration WHERE Id=last_insert_id();",
                entity,
                transaction: _transaction
            );
    }

    public async Task<TaskRegistrationDto?> DeleteByIdAsync(int id)
    {
        var obj = await GetByIdOrDefaultAsync(id);

        if (obj is null)
            return null;

        await _connection.ExecuteAsync(
            "DELETE FROM taskscheduler.taskregistration WHERE Id=@Id;",
            new { id },
            transaction: _transaction
        );

        return obj;
    }

    public async Task<IEnumerable<TaskRegistrationDto>> GetAllAsync()
    {
        return
            await _connection.QueryAsync<TaskRegistrationDto>(
                "SELECT Id, IdTaskGroup, Name, NormalizedName, Location, Description, Author FROM taskscheduler.taskregistration;",
                transaction: _transaction
            );
    }

    public async Task<IEnumerable<TaskRegistrationDto>> GetByGroupAsync(int idTaskGroup)
    {
        return
            await _connection.QueryAsync<TaskRegistrationDto>(
                "SELECT Id, IdTaskGroup, Name, NormalizedName, Location, Description, Author FROM taskscheduler.taskregistration WHERE IdTaskGroup=@IdTaskGroup;",
                new { idTaskGroup },
                transaction: _transaction
            );
    }

    public async Task<TaskRegistrationDto?> GetByIdOrDefaultAsync(int id)
    {
        return
            await _connection.QueryFirstOrDefaultAsync<TaskRegistrationDto?>(
                "SELECT Id, IdTaskGroup, Name, NormalizedName, Location, Description, Author FROM taskscheduler.taskregistration WHERE Id=@Id;",
                new { id },
                transaction: _transaction
            );
    }

    public async Task<TaskRegistrationDto?> GetByNameAsync(string normalizedName)
    {
        return
            await _connection.QueryFirstOrDefaultAsync<TaskRegistrationDto>(
                "SELECT Id, IdTaskGroup, Name, NormalizedName, Location, Description, Author FROM taskscheduler.taskregistration WHERE NormalizedName=@NormalizedName;",
                new { normalizedName },
                transaction: _transaction
            );
    }

    public Task<TaskRegistrationDto?> UpdateAsync(TaskRegistration entity)
    {
        throw new NotImplementedException();
    }
}