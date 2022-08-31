using Dapper;
using TaskSchedulerScraping.Application.Dto.TaskScheduler;
using TaskSchedulerScraping.Application.Repositories.TaskScheduler;
using TaskSchedulerScraping.Domain.Entities.TaskScheduler;
using TaskSchedulerScraping.Infrastructure.UoW;

namespace TaskSchedulerScraping.Infrastructure.Repositories.TaskScheduler;

public class TaskActionRepository : RepositoryBase, ITaskActionRepository
{
    public TaskActionRepository(IUnitOfWorkRepository unitOfWorkRepository) : base(unitOfWorkRepository)
    {
    }

    public async Task<TaskActionDto?> AddAsync(TaskAction entity)
    {
        return
            await _connection.QueryFirstOrDefaultAsync<TaskActionDto?>(
                "INSERT INTO taskscheduler.taskaction (IdTaskRegistration, UpdateAt, ProgramOrScript, Args, StartIn) VALUES (@IdTaskRegistration, @UpdateAt, @ProgramOrScript, @Args, @StartIn);" +
                "SELECT Id, IdTaskRegistration, UpdateAt, ProgramOrScript, Args, StartIn FROM taskscheduler.taskaction WHERE Id=last_insert_id();",
                entity,
                transaction: _transaction
            );
    }

    public async Task<TaskActionDto?> DeleteByIdAsync(int id)
    {
        var obj = await GetByIdOrDefaultAsync(id);

        if (obj is null)
            return null;

        await _connection.ExecuteAsync(
            "DELETE FROM taskscheduler.taskaction WHERE Id=@Id;",
            new { id },
            transaction: _transaction
        );

        return obj;
    }

    public async Task<IEnumerable<TaskActionDto>> GetAllAsync()
    {
        return
            await _connection.QueryAsync<TaskActionDto>(
                "SELECT Id, IdTaskRegistration, UpdateAt, ProgramOrScript, Args, StartIn FROM taskscheduler.taskaction;",
                transaction: _transaction
            );
    }

    public async Task<IEnumerable<TaskActionDto>> GetAllByRegistrationAsync(int idTaskRegistration)
    {
        return
            await _connection.QueryAsync<TaskActionDto>(
                "SELECT Id, IdTaskRegistration, UpdateAt, ProgramOrScript, Args, StartIn FROM taskscheduler.taskaction WHERE IdTaskRegistration=@IdTaskRegistration;",
                new { idTaskRegistration },
                transaction: _transaction
            );
    }

    public async Task<TaskActionDto?> GetByIdOrDefaultAsync(int id)
    {
        return
            await _connection.QueryFirstOrDefaultAsync<TaskActionDto?>(
                "SELECT Id, IdTaskRegistration, UpdateAt, ProgramOrScript, Args, StartIn FROM taskscheduler.taskaction WHERE Id=@Id;",
                new { id },
                transaction: _transaction
            );
    }

    public Task<TaskActionDto?> UpdateAsync(TaskAction entity)
    {
        throw new NotImplementedException();
    }
}