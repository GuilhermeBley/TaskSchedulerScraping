using TaskSchedulerScraping.Application.Services.Interfaces;

namespace TaskSchedulerScraping.Tests.Infrastructure.Tests;

public class ServiceTaskSchedulerTest : InfrastructureTestBase
{
    private readonly ITaskSchedulerService _taskSchedulerService;

    public ServiceTaskSchedulerTest()
    {
        _taskSchedulerService = _serviceProvider.GetService<ITaskSchedulerService>() ??
            throw new ArgumentNullException(nameof(ITaskSchedulerService));
    }

    public async Task Group_AddAndDelete_AddAndDeleteSucess()
    {
        var name = "AddAndDelete_AddAndDeleteSucess";

        var entity = await _taskSchedulerService.GetTaskGroupByNameAsync(name);

        if (entity is null)
        {
            entity =
                new Application.Dto.TaskScheduler.TaskGroupDto
                {
                    CreateAt = DateTime.Now,
                    Name = name
                };

            entity = await _taskSchedulerService.AddTaskGroupAsync(entity);
        }

        var removed = await _taskSchedulerService.DeleteTaskGroupAsync(entity.Id);

        Assert.NotNull(removed);
    }

    public async Task OnSchedule_TryAdd_AllOnSchedulesValues()
    {
        throw new NotImplementedException();
    }

    public async Task Registration_AddAndDelete_AddTwoRegistrationSucess()
    {
        var name = "AddAndDelete_AddAndDeleteSucess";

        var group = await _taskSchedulerService.GetTaskGroupByNameAsync(name);

        if (group is null)
        {
            group =
                new Application.Dto.TaskScheduler.TaskGroupDto
                {
                    CreateAt = DateTime.Now,
                    Name = name
                };

            group = await _taskSchedulerService.AddTaskGroupAsync(group);
            Assert.NotNull(group);
        }

        var registration1 =
            new Application.Dto.TaskScheduler.TaskRegistrationDto
            {
                Author = "me",
                Description = "Test",
                IdTaskGroup = group.Id,
                Location = "Hear",
                Name = "registration1"
            };

        var registration2 =
           new Application.Dto.TaskScheduler.TaskRegistrationDto
           {
               Author = "me",
               Description = "Test",
               IdTaskGroup = group.Id,
               Location = "Hear",
               Name = "registration2"
           };

        var findRegistration1 = await _taskSchedulerService.GetTaskRegistrationByNameAsync(registration1.Name);
        if (findRegistration1 is null)
            Assert.NotNull(await _taskSchedulerService.AddTaskRegistrationAsync(registration1));
        else
            registration1 = findRegistration1;
        var findRegistration2 = await _taskSchedulerService.GetTaskRegistrationByNameAsync(registration2.Name);
        if ((await _taskSchedulerService.GetTaskRegistrationByNameAsync(registration2.Name)) is null)
            Assert.NotNull(await _taskSchedulerService.AddTaskRegistrationAsync(registration2));
        else
            registration2 = findRegistration2;

        await Assert.ThrowsAnyAsync<Application.Exceptions.BadRequestTssException>(() => _taskSchedulerService.DeleteTaskGroupAsync(group.Id));

        var action =
            new Application.Dto.TaskScheduler.TaskActionDto
            {
                Args = "args",
                IdTaskRegistration = registration1.Id,
                ProgramOrScript = "teste.exe",
                StartIn = "time",
                UpdateAt = DateTime.Now
            };
        var trigger = 
            new Application.Dto.TaskScheduler.TaskTriggerDto
            {
                Enabled = true,
                Expire = null,
                IdTaskOnSchedule = 0,
                UpdateAt = DateTime.Now
            };

        Assert.NotNull(await _taskSchedulerService.AddTaskActionAsync(action));
        Assert.NotNull(await _taskSchedulerService.AddTaskTriggerAsync(trigger));
    }
}