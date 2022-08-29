using TaskSchedulerScraping.Domain.Validations;

namespace TaskSchedulerScraping.Domain.Entities.TaskScheduler;

public class TaskTrigger
{
    public int Id { get; private set; }
    public int IdTaskRegistration { get; private set; }
    public DateTime UpdateAt { get; private set; }
    public bool Enabled { get; private set; }
    public DateTime Start { get; private set; }
    public int IdTaskOnSchedule { get; private set; }
    public DateTime? Expire { get; private set; }

    private TaskTrigger(
        int id,
        int idTaskRegistration,
        DateTime updateAt,
        bool enabled,
        DateTime start,
        int idTaskOnSchedule,
        DateTime? expire)
    {
        Id = id;
        IdTaskRegistration = idTaskRegistration;
        UpdateAt = updateAt;
        Enabled = enabled;
        Start = start;
        IdTaskOnSchedule = idTaskOnSchedule;
        Expire = expire;
    }

    public static IValidationResult<TaskTrigger> Create(
        int id,
        int idTaskRegistration,
        DateTime updateAt,
        bool enabled,
        DateTime start,
        int idTaskOnSchedule,
        DateTime? expire)
    {
        var execeptions = new List<string>();

        if (id < 0)
            execeptions.Add($"{nameof(id)} must be greater or equals than '0'.");

        if (idTaskRegistration < 1)
            execeptions.Add($"{nameof(idTaskRegistration)} must be greater than '0'.");
            
        if (idTaskOnSchedule < 1)
            execeptions.Add($"{nameof(idTaskOnSchedule)} must be greater than '0'.");

        if (start.Equals(DateTime.MinValue))
            execeptions.Add($"{nameof(updateAt)} must be greater than {DateTime.MinValue}.");

        if (expire is not null && expire > DateTime.Now)
            execeptions.Add($"{nameof(expire)} must be greater than {DateTime.Now}.");

        if (execeptions.Any())
            return ValidationResult<TaskTrigger>.GetWithErrors(execeptions);

        return ValidationResult<TaskTrigger>.GetSuccess(
            new TaskTrigger(id, idTaskRegistration, updateAt, enabled, start, idTaskOnSchedule, expire)
        );
    }
}