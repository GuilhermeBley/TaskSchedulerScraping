using Dapper;
using TaskSchedulerScraping.Application.Dto.TaskScheduler;
using TaskSchedulerScraping.Application.Repositories.TaskScheduler;
using TaskSchedulerScraping.Domain.Entities.TaskScheduler;
using TaskSchedulerScraping.Infrastructure.UoW;

namespace TaskSchedulerScraping.Infrastructure.Repositories.TaskScheduler;

public class TaskOnScheduleRepository : RepositoryBase, ITaskOnScheduleRepository
{
    public TaskOnScheduleRepository(IUnitOfWorkRepository unitOfWorkRepository) : base(unitOfWorkRepository)
    {
    }

    public async Task<TaskOnScheduleDto?> AddAsync(TaskOnSchedule entity)
    {
        return
            await _connection.QueryFirstOrDefaultAsync<TaskOnScheduleDto?>(
                "INSERT INTO taskscheduler.taskonschedule (Id, Name) VALUES (@Id, @Name);" +
                "SELECT Id, Name FROM taskscheduler.taskonschedule WHERE Id=last_insert_id();",
                entity,
                transaction: _transaction
            );
    }

    public async Task<TaskOnScheduleDto?> DeleteByIdAsync(int id)
    {
        var obj = await GetByIdOrDefaultAsync(id);

        if (obj is null)
            return null;

        await _connection.ExecuteAsync(
            "DELETE FROM taskscheduler.taskonschedule WHERE Id=@Id;",
            new { id },
            transaction: _transaction
        );

        return obj;
    }

    public async Task<IEnumerable<TaskOnScheduleDto>> GetAllAsync()
    {
        return
            await _connection.QueryAsync<TaskOnScheduleDto>(
                "SELECT Id, Name FROM taskscheduler.taskonschedule;",
                transaction: _transaction
            );
    }

    public async Task<TaskOnScheduleDto?> GetByIdOrDefaultAsync(int id)
    {
        return
            await _connection.QueryFirstOrDefaultAsync<TaskOnScheduleDto>(
                "SELECT Id, Name FROM taskscheduler.taskonschedule WHERE Id=@Id;",
                new { id },
                transaction: _transaction
            );
    }

    public async Task<TaskOnScheduleDto?> TryAddAsync(TaskOnSchedule taskOnSchedule)
    {
        if ((await GetByIdOrDefaultAsync(taskOnSchedule.Id)) is null)
        {
            return await AddAsync(taskOnSchedule);
        }

        return null;
    }

    public Task<TaskOnScheduleDto?> UpdateAsync(TaskOnSchedule entity)
    {
        throw new NotImplementedException();
    }
}