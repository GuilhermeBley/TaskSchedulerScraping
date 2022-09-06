using AutoMapper;
using TaskSchedulerScraping.Application.Dto.TaskScheduler;
using TaskSchedulerScraping.Application.Exceptions;
using TaskSchedulerScraping.Application.Repositories.TaskScheduler;
using TaskSchedulerScraping.Application.Services.Interfaces;
using TaskSchedulerScraping.Application.UoW;
using TaskSchedulerScraping.Domain.Entities.TaskScheduler;

namespace TaskSchedulerScraping.Application.Services.Implementation;

public sealed class TaskSchedulerService : ITaskSchedulerService
{
    private readonly ITaskGroupRepository _taskGroupRepository;
    private readonly ITaskActionRepository _taskActionRepository;
    private readonly ITaskTriggerRepository _taskTriggerRepository;
    private readonly ITaskRegistrationRepository _taskRegistrationRepository;
    private readonly ITaskOnScheduleRepository _taskOnScheduleRepository;
    private readonly IUnitOfWork _uoW;
    private readonly IMapper _mapper;
    private static readonly IEnumerable<TaskOnScheduleDto> _allOnSchedule = GetAllOnScheduleStatic();

    public TaskSchedulerService(
        ITaskGroupRepository taskGroupRepository,
        ITaskActionRepository taskActionRepository,
        ITaskTriggerRepository taskTriggerRepository,
        ITaskRegistrationRepository taskRegistrationRepository,
        ITaskOnScheduleRepository taskOnScheduleRepository,
        IUnitOfWork uoW,
        IMapper mapper)
    {
        _taskGroupRepository = taskGroupRepository;
        _taskActionRepository = taskActionRepository;
        _taskTriggerRepository = taskTriggerRepository;
        _taskRegistrationRepository = taskRegistrationRepository;
        _taskOnScheduleRepository = taskOnScheduleRepository;
        _uoW = uoW;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TaskOnScheduleDto>> AddAllOnShceduleAsync()
    {
        var listOnSchedule = _allOnSchedule.Select(dto => _mapper.Map<TaskOnSchedule>(dto));

        using (await _uoW.BeginTransactionAsync())
        {
            foreach (var onSchedule in listOnSchedule)
            {
                await _taskOnScheduleRepository.TryAddAsync(onSchedule);
            }

            await _uoW.SaveChangesAsync();
        }

        using (await _uoW.OpenConnectionAsync())
            return await _taskOnScheduleRepository.GetAllAsync();
    }

    public async Task<TaskActionDto> AddTaskActionAsync(TaskActionDto taskAction)
    {
        var taskActionMapped = _mapper.Map<TaskAction>(taskAction);

        using (await _uoW.BeginTransactionAsync())
        {
            taskAction =
                (await _taskActionRepository.AddAsync(taskActionMapped)) ??
                throw new BadRequestTssException("Failed to insert new Task Action.");

            await _uoW.SaveChangesAsync();
        }

        return taskAction;
    }

    public async Task<TaskGroupDto> AddTaskGroupAsync(TaskGroupDto taskGroup)
    {
        var taskGroupMapped = _mapper.Map<TaskGroup>(taskGroup);

        using (await _uoW.BeginTransactionAsync())
        {
            if ((await _taskGroupRepository.GetByNameAsync(taskGroupMapped.NormalizedName)) is not null)
                throw new ConflictTssException($"Task group with name {taskGroupMapped.NormalizedName} already exists.");

            taskGroup =
                (await _taskGroupRepository.AddAsync(taskGroupMapped)) ??
                throw new BadRequestTssException("Failed to insert new Task Group.");

            await _uoW.SaveChangesAsync();
        }

        return taskGroup;
    }

    public async Task<TaskRegistrationDto> AddTaskRegistrationAsync(TaskRegistrationDto taskRegistration)
    {
        var taskRegistrationMapped = _mapper.Map<TaskRegistration>(taskRegistration);

        using (await _uoW.BeginTransactionAsync())
        {
            if ((await _taskRegistrationRepository.GetByNameAsync(taskRegistrationMapped.NormalizedName)) is not null)
                throw new ConflictTssException($"Task registration with name {taskRegistrationMapped.NormalizedName} already exists.");

            taskRegistration =
                (await _taskRegistrationRepository.AddAsync(taskRegistrationMapped)) ??
                throw new BadRequestTssException("Failed to insert new Task Registration.");

            await _uoW.SaveChangesAsync();
        }

        return taskRegistration;
    }

    public async Task<TaskTriggerDto> AddTaskTriggerAsync(TaskTriggerDto taskTrigger)
    {
        var taskTriggerMapped = _mapper.Map<TaskTrigger>(taskTrigger);

        using (await _uoW.BeginTransactionAsync())
        {
            taskTrigger =
                (await _taskTriggerRepository.AddAsync(taskTriggerMapped)) ??
                throw new BadRequestTssException("Failed to insert new task trigger.");
            await _uoW.SaveChangesAsync();
        }

        return taskTrigger;
    }

    public async Task<TaskActionDto> DeleteTaskActionAsync(int idTaskAction)
    {
        using (await _uoW.BeginTransactionAsync())
        {
            var taskAction =
                await _taskActionRepository.GetByIdOrDefaultAsync(idTaskAction) ??
                throw new NotFoundTssException($"Task Action with Id '{idTaskAction}' not found.");

            await _taskActionRepository.DeleteByIdAsync(idTaskAction);
            await _uoW.SaveChangesAsync();

            return taskAction;
        }
    }

    public async Task<TaskGroupDto> DeleteTaskGroupAsync(int idTaskGroup, bool confirmRelatioshipsDeletion = false)
    {
        using (await _uoW.BeginTransactionAsync())
        {
            var taskGroup =
                await _taskGroupRepository.GetByIdOrDefaultAsync(idTaskGroup) ??
                throw new NotFoundTssException($"Task Group with Id '{idTaskGroup}' not found.");

            var taskRegistrations = await _taskRegistrationRepository.GetByGroupAsync(idTaskGroup);

            if (taskRegistrations.Any() && !confirmRelatioshipsDeletion)
            {
                throw new BadRequestTssException($"Has related data in Task Registration with Task Group '{taskGroup.Name}'," +
                " please add a confirmation relatioships deletion or delete manually.");
            }

            foreach (var taskRegistration in taskRegistrations)
                await WithOutConnDeleteTaskRegistrationAsync(taskRegistration.Id, confirmRelatioshipsDeletion);

            if ((await _taskGroupRepository.DeleteByIdAsync(idTaskGroup)) is null)
                throw new BadRequestTssException("Failed to delete Task Task Group.");

            await _uoW.SaveChangesAsync();

            return taskGroup;
        }
    }

    public async Task<TaskRegistrationDto> DeleteTaskRegistrationAsync(int idTaskRegistration, bool confirmRelatioshipsDeletion = false)
    {
        using (await _uoW.BeginTransactionAsync())
        {
            var taskRegistration = await WithOutConnDeleteTaskRegistrationAsync(idTaskRegistration, confirmRelatioshipsDeletion);

            await _uoW.SaveChangesAsync();

            return taskRegistration;
        }
    }

    public async Task<TaskTriggerDto> DeleteTaskTriggerAsync(int idTaskTrigger)
    {
        using (await _uoW.BeginTransactionAsync())
        {
            var taskAction =
                await _taskTriggerRepository.GetByIdOrDefaultAsync(idTaskTrigger) ??
                throw new NotFoundTssException($"Task Trigger with Id '{idTaskTrigger}' not found.");

            await _taskTriggerRepository.DeleteByIdAsync(idTaskTrigger);

            await _uoW.SaveChangesAsync();

            return taskAction;
        }
    }

    public async Task<IEnumerable<TaskOnScheduleDto>> GetAllOnScheduleAsync()
    {
        await Task.CompletedTask;
        return _allOnSchedule;
    }

    public async Task<IEnumerable<TaskGroupDto>> GetAllTaskGroupAsync()
    {
        using (await _uoW.OpenConnectionAsync())
            return await _taskGroupRepository.GetAllAsync();
    }

    public async Task<IEnumerable<TaskActionDto>> GetTaskActionByRegistrationAsync(int idTaskRegistration)
    {
        using (await _uoW.OpenConnectionAsync())
            return await _taskActionRepository.GetAllByRegistrationAsync(idTaskRegistration);
    }

    public async Task<TaskGroupDto> GetTaskGroupByNameAsync(string normalizedName)
    {
        using (await _uoW.OpenConnectionAsync())
            return
                (await _taskGroupRepository.GetByNameAsync(normalizedName)) ??
                throw new NotFoundTssException($"Task Group named by {normalizedName} could not be found.");
    }

    public async Task<IEnumerable<TaskRegistrationDto>> GetTaskRegistrationByGroupAsync(int idTaskGroup)
    {
        using (await _uoW.OpenConnectionAsync())
            return await _taskRegistrationRepository.GetByGroupAsync(idTaskGroup);
    }

    public async Task<TaskRegistrationDto> GetTaskRegistrationByNameAsync(string normalizedName)
    {
        using (await _uoW.OpenConnectionAsync())
            return
                (await _taskRegistrationRepository.GetByNameAsync(normalizedName)) ??
                throw new NotFoundTssException($"Task Registration named by {normalizedName} could not be found.");
    }

    public async Task<IEnumerable<TaskTriggerDto>> GetTaskTriggerByRegistrationAsync(int idTaskRegistration)
    {
        using (await _uoW.OpenConnectionAsync())
            return await _taskTriggerRepository.GetAllByRegistrationAsync(idTaskRegistration);
    }

    private async Task<TaskRegistrationDto> WithOutConnDeleteTaskRegistrationAsync(int idTaskRegistration, bool confirmRelatioshipsDeletion)
    {
        var taskRegistration =
                await _taskRegistrationRepository.GetByIdOrDefaultAsync(idTaskRegistration) ??
                throw new NotFoundTssException($"Task Registration with Id '{idTaskRegistration}' not found.");

        var taskActions = await _taskActionRepository.GetAllByRegistrationAsync(idTaskRegistration);

        if (taskActions.Any() && !confirmRelatioshipsDeletion)
        {
            throw new BadRequestTssException($"Has related data in Task Actions with Task Registration '{taskRegistration.Name}'," +
                " please add a confirmation relatioships deletion or delete manually.");
        }

        foreach (var taskAction in taskActions)
            await _taskActionRepository.DeleteByIdAsync(taskAction.Id);

        var taskTriggers = await _taskTriggerRepository.GetAllByRegistrationAsync(idTaskRegistration);

        if (taskTriggers.Any() && !confirmRelatioshipsDeletion)
        {
            throw new BadRequestTssException($"Has related data in Task Trigger with Task Registration '{taskRegistration.Name}'," +
                " please add a confirmation relatioships deletion or delete manually.");
        }

        foreach (var taskTrigger in taskTriggers)
            await _taskTriggerRepository.DeleteByIdAsync(taskTrigger.Id);

        return
            (await _taskRegistrationRepository.DeleteByIdAsync(taskRegistration.Id)) ??
            throw new BadRequestTssException("Failed to delete Task Registration.");
    }

    private static IEnumerable<TaskOnScheduleDto> GetAllOnScheduleStatic()
    {
        var listTaskOnSchedule = new List<TaskOnScheduleDto>();
        listTaskOnSchedule.Add(new TaskOnScheduleDto
            {
                Id = 1,
                Name = "OneTime"
            });
        listTaskOnSchedule.Add(new TaskOnScheduleDto
            {
                Id = 2,
                Name = "Daily"
            });
        listTaskOnSchedule.Add(new TaskOnScheduleDto
            {
                Id = 3,
                Name = "Weekly"
            });
        listTaskOnSchedule.Add(new TaskOnScheduleDto
            {
                Id = 4,
                Name = "Monthly"
            });
        return listTaskOnSchedule;
    }
}