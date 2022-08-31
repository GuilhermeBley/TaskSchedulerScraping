using TaskSchedulerScraping.Domain.Validations;

namespace TaskSchedulerScraping.Domain.Entities.TaskScheduler;

public class TaskOnSchedule
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private TaskOnSchedule(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public static IValidationResult<TaskOnSchedule> Create(int id, string name)
    {
        var execeptions = new List<string>();

        if (id < 1)
            execeptions.Add($"{nameof(id)} must be greater than '0'.");

        if (string.IsNullOrEmpty(name))
            execeptions.Add($"Has null or empty required strings.");

        if (!name.All(char.IsLetter))
            execeptions.Add($"{nameof(name)} must be have only letters.");

        if (execeptions.Any())
            return ValidationResult<TaskOnSchedule>.GetWithErrors(execeptions);

        return ValidationResult<TaskOnSchedule>.GetSuccess(
            new TaskOnSchedule(id, name)
        );
    }
}