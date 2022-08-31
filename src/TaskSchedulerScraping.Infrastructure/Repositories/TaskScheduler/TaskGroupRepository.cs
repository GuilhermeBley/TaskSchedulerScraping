using Dapper;
using TaskSchedulerScraping.Application.Dto.TaskScheduler;
using TaskSchedulerScraping.Application.Repositories.TaskScheduler;
using TaskSchedulerScraping.Domain.Entities.TaskScheduler;
using TaskSchedulerScraping.Infrastructure.UoW;

namespace TaskSchedulerScraping.Infrastructure.Repositories.TaskScheduler;

public class TaskGroupRepository : RepositoryBase, ITaskGroupRepository
{
    public TaskGroupRepository(IUnitOfWorkRepository unitOfWorkRepository) : base(unitOfWorkRepository)
    {
    }

    public async Task<TaskGroupDto?> AddAsync(TaskGroup entity)
    {
        return
            await _connection.QueryFirstOrDefaultAsync<TaskGroupDto?>(
                "INSERT INTO taskscheduler.taskgroup (Name, NormalizedName, CreateAt) VALUES (@Name, @NormalizedName, @CreateAt);" +
                "SELECT Id, Name, NormalizedName, CreateAt FROM taskscheduler.taskgroup WHERE Id=last_insert_id();",
                entity,
                transaction: _transaction
            );
    }

    public async Task<TaskGroupDto?> DeleteByIdAsync(int id)
    {
        var obj = await GetByIdOrDefaultAsync(id);

        if (obj is null)
            return null;

        await _connection.ExecuteAsync(
            "DELETE FROM taskscheduler.taskgroup WHERE Id=@Id;",
            new { id },
            transaction: _transaction
        );

        return obj;
    }

    public async Task<IEnumerable<TaskGroupDto>> GetAllAsync()
    {
        return
            await _connection.QueryAsync<TaskGroupDto>(
                "SELECT Id, Name, NormalizedName, CreateAt FROM taskscheduler.taskgroup;",
                transaction: _transaction
            );
    }

    public async Task<TaskGroupDto?> GetByIdOrDefaultAsync(int id)
    {
        return
            await _connection.QueryFirstOrDefaultAsync<TaskGroupDto?>(
                "SELECT Id, Name, NormalizedName, CreateAt FROM taskscheduler.taskgroup WHERE Id=@Id;",
                new { id },
                transaction: _transaction
            );
    }

    public async Task<TaskGroupDto?> GetByNameAsync(string normalizedName)
    {
        return
            await _connection.QueryFirstOrDefaultAsync<TaskGroupDto?>(
                "SELECT Id, Name, NormalizedName, CreateAt FROM taskscheduler.taskgroup WHERE NormalizedName=@NormalizedName;",
                new { normalizedName },
                transaction: _transaction
            );
    }

    public Task<TaskGroupDto?> UpdateAsync(TaskGroup entity)
    {
        throw new NotImplementedException();
    }
}