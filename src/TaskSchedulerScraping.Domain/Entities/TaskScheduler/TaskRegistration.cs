using TaskSchedulerScraping.Domain.Validations;

namespace TaskSchedulerScraping.Domain.Entities.TaskScheduler;

public class TaskRegistration
{
    public int Id { get; private set; }
    public int IdTaskGroup { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;

    private TaskRegistration(
        int id,
        int idTaskGroup,
        string name,
        string normalizedName,
        string location,
        string description,
        string author)
    {
        Id = id;
        IdTaskGroup = idTaskGroup;
        Name = name;
        NormalizedName = normalizedName;
        Location = location;
        Description = description;
        Author = author;
    }

    public static IValidationResult<TaskRegistration> Create(
        int id,
        int idTaskGroup,
        string name,
        string location,
        string description,
        string author)
    {
        var execeptions = new List<string>();

        if (id < 0)
            execeptions.Add($"{nameof(id)} must be greater or equals than '0'.");

        if (string.IsNullOrEmpty(name) ||
            string.IsNullOrEmpty(location) ||
            string.IsNullOrEmpty(description) ||
            string.IsNullOrEmpty(author))
            execeptions.Add($"Has null or empty required strings.");

        if (idTaskGroup < 1)
            execeptions.Add($"{nameof(idTaskGroup)} must be greater than '0'.");

        if (!name.All(char.IsLetter))
            execeptions.Add($"{nameof(name)} must be have only letters.");

        if (execeptions.Any())
            return ValidationResult<TaskRegistration>.GetWithErrors(execeptions);

        return ValidationResult<TaskRegistration>.GetSuccess(
            new TaskRegistration(id, idTaskGroup, name, name.ToUpper(), location, description, author)
        );
    }
}