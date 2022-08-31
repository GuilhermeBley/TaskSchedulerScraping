using Dapper;
using TaskSchedulerScraping.Application.Dto.TaskScheduler;
using TaskSchedulerScraping.Application.Repositories.TaskScheduler;
using TaskSchedulerScraping.Domain.Entities.TaskScheduler;
using TaskSchedulerScraping.Infrastructure.UoW;

namespace TaskSchedulerScraping.Infrastructure.Repositories.TaskScheduler;

public class TaskTriggerRepository : RepositoryBase, ITaskTriggerRepository
{
    public TaskTriggerRepository(IUnitOfWorkRepository unitOfWorkRepository) : base(unitOfWorkRepository)
    {
    }

    public async Task<TaskTriggerDto?> AddAsync(TaskTrigger entity)
    {
        return
            await _connection.QueryFirstOrDefaultAsync<TaskTriggerDto?>(
                "INSERT INTO taskscheduler.tasktrigger (IdTaskRegistration, UpdateAt, Enabled, Start, IdTaskOnSchedule, Expire) VALUES (@IdTaskRegistration, @UpdateAt, @Enabled, @Start, @IdTaskOnSchedule, @Expire);" +
                "SELECT Id, IdTaskRegistration, UpdateAt, Enabled, Start, IdTaskOnSchedule, Expire FROM taskscheduler.tasktrigger WHERE Id=last_insert_id();",
                entity,
                transaction: _transaction
            );
    }

    public async Task<TaskTriggerDto?> DeleteByIdAsync(int id)
    {
        var obj = await GetByIdOrDefaultAsync(id);

        if (obj is null)
            return null;

        await _connection.ExecuteAsync(
            "DELETE FROM taskscheduler.tasktrigger WHERE Id=@Id;",
            new { id },
            transaction: _transaction
        );

        return obj;
    }

    public async Task<IEnumerable<TaskTriggerDto>> GetAllAsync()
    {
        return
            await _connection.QueryAsync<TaskTriggerDto>(
                "SELECT Id, IdTaskRegistration, UpdateAt, Enabled, Start, IdTaskOnSchedule, Expire FROM taskscheduler.tasktrigger;",
                transaction: _transaction
            );
    }

    public async Task<IEnumerable<TaskTriggerDto>> GetAllByRegistrationAsync(int idTaskRegistration)
    {
        return
            await _connection.QueryAsync<TaskTriggerDto>(
                "SELECT Id, IdTaskRegistration, UpdateAt, Enabled, Start, IdTaskOnSchedule, Expire FROM taskscheduler.tasktrigger WHERE IdTaskRegistration=@IdTaskRegistration;",
                new { idTaskRegistration },
                transaction: _transaction
            );
    }

    public async Task<TaskTriggerDto?> GetByIdOrDefaultAsync(int id)
    {
        return
            await _connection.QuerySingleOrDefaultAsync<TaskTriggerDto>(
                "SELECT Id, IdTaskRegistration, UpdateAt, Enabled, Start, IdTaskOnSchedule, Expire FROM taskscheduler.tasktrigger WHERE Id=@Id;",
                new { id },
                transaction: _transaction
            );
    }

    public Task<TaskTriggerDto?> UpdateAsync(TaskTrigger entity)
    {
        throw new NotImplementedException();
    }
}