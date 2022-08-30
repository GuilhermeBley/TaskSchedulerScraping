using TaskSchedulerScraping.Application.Dto.TaskScheduler;
using TaskSchedulerScraping.Domain.Entities.TaskScheduler;

namespace TaskSchedulerScraping.Application.Repositories.TaskScheduler;

public interface ITaskRegistrationRepository : IRepositoryBase<TaskRegistration, TaskRegistrationDto, int>
{
    Task<IEnumerable<TaskRegistrationDto>> GetByGroupAsync(int idTaskGroup);
    Task<TaskRegistrationDto?> GetByNameAsync(string normalizedName);
}