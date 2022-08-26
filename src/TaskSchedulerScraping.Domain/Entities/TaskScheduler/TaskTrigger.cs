using TaskSchedulerScraping.Domain.Validations;

namespace TaskSchedulerScraping.Domain.Entities.TaskScheduler;

public class TaskTrigger
{
    public int Id { get; private set; }
    public int IdTaskRegistration { get; private set; }
    public TaskRegistration TaskRegistration { get; private set; } = null!;
    public DateTime UpdateAt { get; private set; }
    public bool Enabled { get; private set; }
    public DateTime Start { get; private set; }
    public int IdTaskOnSchedule { get; private set; }
    public TaskOnSchedule TaskOnSchedule { get; private set; }
    public DateTime? Expire { get; private set; }

    private TaskTrigger(
        int id,
        int idTaskRegistration,
        TaskRegistration taskRegistration,
        DateTime updateAt,
        bool enabled,
        DateTime start,
        int idTaskOnSchedule,
        TaskOnSchedule taskOnSchedule,
        DateTime? expire)
    {
        Id = id;
        IdTaskRegistration = idTaskRegistration;
        TaskRegistration = taskRegistration;
        UpdateAt = updateAt;
        Enabled = enabled;
        Start = start;
        IdTaskOnSchedule = idTaskOnSchedule;
        TaskOnSchedule = taskOnSchedule;
        Expire = expire;
    }

    public static IValidationResult<TaskTrigger> Create(
        int id,
        int idTaskRegistration,
        TaskRegistration taskRegistration,
        DateTime updateAt,
        bool enabled,
        DateTime start,
        int idTaskOnSchedule,
        TaskOnSchedule taskOnSchedule,
        DateTime? expire)
    {
        var execeptions = new List<string>();

        if (id < 0)
            execeptions.Add($"{nameof(id)} must be greater than '0'.");

        if (taskRegistration is null || idTaskRegistration < 1)
            execeptions.Add($"{nameof(taskRegistration)} is null.");
            
        if (taskOnSchedule is null || idTaskOnSchedule < 1)
            execeptions.Add($"{nameof(taskOnSchedule)} is null.");

        if (start.Equals(DateTime.MinValue))
            execeptions.Add($"{nameof(updateAt)} must be greater than {DateTime.MinValue}.");

        if (expire is not null && expire > DateTime.Now)
            execeptions.Add($"{nameof(expire)} must be greater than {DateTime.Now}.");

        if (execeptions.Any())
            return ValidationResult<TaskTrigger>.GetWithErrors(execeptions);

        return ValidationResult<TaskTrigger>.GetSuccess(
            new TaskTrigger(id, idTaskRegistration, taskRegistration!, updateAt, enabled, start, idTaskOnSchedule, taskOnSchedule!, expire)
        );
    }
}