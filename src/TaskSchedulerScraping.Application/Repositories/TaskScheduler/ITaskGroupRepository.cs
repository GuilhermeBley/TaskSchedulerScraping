using TaskSchedulerScraping.Application.Dto.TaskScheduler;
using TaskSchedulerScraping.Domain.Entities.TaskScheduler;

namespace TaskSchedulerScraping.Application.Repositories.TaskScheduler;

public interface ITaskGroupRepository : IRepositoryBase<TaskGroup, TaskGroupDto, int>
{

}