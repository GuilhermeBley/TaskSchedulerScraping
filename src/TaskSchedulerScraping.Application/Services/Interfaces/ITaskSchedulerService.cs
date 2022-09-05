using TaskSchedulerScraping.Application.Dto.TaskScheduler;

namespace TaskSchedulerScraping.Application.Services.Interfaces;

public interface ITaskSchedulerService
{
    Task<IEnumerable<TaskOnScheduleDto>> AddAllOnShceduleAsync();
    Task<TaskActionDto> AddTaskActionAsync(TaskActionDto taskAction);
    Task<TaskGroupDto> AddTaskGroupAsync(TaskGroupDto taskGroup);
    Task<TaskRegistrationDto> AddTaskRegistrationAsync(TaskRegistrationDto taskRegistration);
    Task<TaskTriggerDto> AddTaskTriggerAsync(TaskTriggerDto taskTrigger);

    Task<TaskActionDto> DeleteTaskActionAsync(int idTaskAction);
    Task<TaskGroupDto> DeleteTaskGroupAsync(int idTaskGroup, bool confirmRelatioshipsDeletion = false);
    Task<TaskRegistrationDto> DeleteTaskRegistrationAsync(int idTaskRegistration, bool confirmRelatioshipsDeletion = false);
    Task<TaskTriggerDto> DeleteTaskTriggerAsync(int idTaskTrigger);

    Task<IEnumerable<TaskGroupDto>> GetAllTaskGroupAsync();
    Task<IEnumerable<TaskOnScheduleDto>> GetAllOnScheduleAsync();
    Task<TaskGroupDto> GetTaskGroupByNameAsync(string normalizedName);
    Task<IEnumerable<TaskActionDto>> GetTaskActionByRegistrationAsync(int idTaskRegistration);
    Task<IEnumerable<TaskRegistrationDto>> GetTaskRegistrationByGroupAsync(int idTaskGroup);
    Task<TaskRegistrationDto> GetTaskRegistrationByNameAsync(string normalizedName);
    Task<IEnumerable<TaskTriggerDto>> GetTaskTriggerByRegistrationAsync(int idTaskRegistration);
}