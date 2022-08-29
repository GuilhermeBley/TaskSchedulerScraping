using TaskSchedulerScraping.Application.Dto.TaskScheduler;
using TaskSchedulerScraping.Application.Repositories.TaskScheduler;
using TaskSchedulerScraping.Application.UoW;

namespace TaskSchedulerScraping.Application.Services.Interfaces;

public class TaskSchedulerService : ITaskSchedulerService
{
    private readonly ITaskGroupRepository _taskGroupRepository;
    private readonly ITaskActionRepository _taskActionRepository;
    private readonly ITaskTriggerRepository _taskTriggerRepository;
    private readonly ITaskSchedulerService _taskSchedulerService;
    private readonly IUnitOfWork _uoW;

    public TaskSchedulerService(
        ITaskGroupRepository taskGroupRepository,
        ITaskActionRepository taskActionRepository,
        ITaskTriggerRepository taskTriggerRepository,
        ITaskSchedulerService taskSchedulerService,
        IUnitOfWork uoW)
    {
        _taskGroupRepository = taskGroupRepository;
        _taskActionRepository = taskActionRepository;
        _taskTriggerRepository = taskTriggerRepository;
        _taskSchedulerService = taskSchedulerService;
        _uoW = uoW;
    }

    public Task<TaskActionDto> AddTaskActionAsync(TaskActionDto taskAction)
    {
        throw new NotImplementedException();
    }

    public Task<TaskGroupDto> AddTaskGroupAsync(TaskGroupDto taskGroup)
    {
        throw new NotImplementedException();
    }

    public Task<TaskRegistrationDto> AddTaskRegistrationAsync(TaskRegistrationDto taskRegistration)
    {
        throw new NotImplementedException();
    }

    public Task<TaskTriggerDto> AddTaskTriggerAsync(TaskTriggerDto taskTrigger)
    {
        throw new NotImplementedException();
    }

    public Task<TaskActionDto> DeleteTaskActionAsync(int idTaskAction)
    {
        throw new NotImplementedException();
    }

    public Task<TaskGroupDto> DeleteTaskGroupAsync(int idTaskGroup, bool confirmRelatioshipsDeletion = false)
    {
        throw new NotImplementedException();
    }

    public Task<TaskRegistrationDto> DeleteTaskRegistrationAsync(int idTaskRegistration, bool confirmRelatioshipsDeletion = false)
    {
        throw new NotImplementedException();
    }

    public Task<TaskTriggerDto> DeleteTaskTriggerAsync(int idTaskTrigger)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TaskGroupDto>> GetAllTaskGroupAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TaskActionDto>> GetTaskActionByRegistrationAsync(int idTaskRegistration)
    {
        throw new NotImplementedException();
    }

    public Task<TaskGroupDto> GetTaskGroupByNameAsync(string normalizedName)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TaskRegistrationDto>> GetTaskRegistrationByGroupAsync(int idTaskGroup)
    {
        throw new NotImplementedException();
    }

    public Task<TaskRegistrationDto> GetTaskRegistrationByNameAsync(string normalizedName)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TaskTriggerDto>> GetTaskTriggerByRegistrationAsync(int idTaskRegistration)
    {
        throw new NotImplementedException();
    }
}