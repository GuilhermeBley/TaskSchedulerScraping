using TaskSchedulerScraping.Application.Dto.TaskScheduler;
using TaskSchedulerScraping.Domain.Entities.TaskScheduler;

namespace TaskSchedulerScraping.Application.Repositories.TaskScheduler;

public interface ITaskActionRepository : IRepositoryBase<TaskAction, TaskActionDto, int>
{
    Task<IEnumerable<TaskActionDto>> GetAllByRegistrationAsync(int idTaskRegistration);
}