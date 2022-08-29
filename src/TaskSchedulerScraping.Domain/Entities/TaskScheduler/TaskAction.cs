using TaskSchedulerScraping.Domain.Validations;

namespace TaskSchedulerScraping.Domain.Entities.TaskScheduler;

public class TaskAction
{
    public int Id { get; private set; }
    public int IdTaskRegistration { get; private set; }
    public DateTime UpdateAt { get; private set; }
    public string ProgramOrScript { get; private set; } = string.Empty;
    public string Args { get; private set; } = string.Empty;
    public string StartIn { get; private set; } = string.Empty;

    private TaskAction(
        int id, 
        int idTaskRegistration, 
        DateTime updateAt, 
        string programOrScript, 
        string args, 
        string startIn)
    {
        Id = id;
        IdTaskRegistration = idTaskRegistration;
        UpdateAt = updateAt;
        ProgramOrScript = programOrScript;
        Args = args;
        StartIn = startIn;
    }

    public static IValidationResult<TaskAction> Create(
        int id, 
        int idTaskRegistration,
        DateTime updateAt, 
        string programOrScript, 
        string args, 
        string startIn)
    {
        var execeptions = new List<string>();

        if (id < 0)
            execeptions.Add($"{nameof(id)} must be greater than '0'.");

        if (string.IsNullOrEmpty(programOrScript) || 
            string.IsNullOrEmpty(args) || 
            string.IsNullOrEmpty(startIn))
            execeptions.Add($"Has null or empty required strings.");

        if (idTaskRegistration < 1)
            execeptions.Add($"{nameof(idTaskRegistration)} must be greater than '0'.");

        if (updateAt.Equals(DateTime.MinValue) ||
            updateAt >= DateTime.Now)
            execeptions.Add($"{nameof(updateAt)} must be greater than {DateTime.MinValue} and less than {DateTime.Now}.");

        if (execeptions.Any())
            return ValidationResult<TaskAction>.GetWithErrors(execeptions);

        return ValidationResult<TaskAction>.GetSuccess(
            new TaskAction(id, idTaskRegistration, updateAt, programOrScript, args, startIn)
        );
    }
}