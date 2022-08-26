using TaskSchedulerScraping.Domain.Validations;

namespace TaskSchedulerScraping.Domain.Entities.TaskScheduler;

public class TaskGroup
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public DateTime CreateAt { get; private set; }

    private TaskGroup(
        int id, 
        string name, 
        string normalizedName,
        DateTime createAt)
    {
        Id = id;
        Name = name;
        NormalizedName = normalizedName;
        CreateAt = createAt;
    }

    public static IValidationResult<TaskGroup> Create(
        int id, 
        string name,
        DateTime createAt)
    {
        var execeptions = new List<string>();

        if (id < 0)
            execeptions.Add($"{nameof(id)} must be greater than '0'.");

        if (string.IsNullOrEmpty(name))
            execeptions.Add($"Has null or empty required strings.");

        if (createAt.Equals(DateTime.MinValue) ||
            createAt >= DateTime.Now)
            execeptions.Add($"{nameof(createAt)} must be greater than {DateTime.MinValue} and less than {DateTime.Now}.");

        if (name.All(char.IsLetter))
            execeptions.Add($"{nameof(name)} must be have only letters.");

        if (execeptions.Any())
            return ValidationResult<TaskGroup>.GetWithErrors(execeptions);

        return ValidationResult<TaskGroup>.GetSuccess(
            new TaskGroup(id, name, name.ToUpper(), createAt)
        );
    }
}