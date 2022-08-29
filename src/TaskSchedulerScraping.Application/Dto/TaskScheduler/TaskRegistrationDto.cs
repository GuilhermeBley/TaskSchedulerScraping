namespace TaskSchedulerScraping.Application.Dto.TaskScheduler;

public class TaskRegistrationDto
{
    public int Id { get; private set; }
    public int IdTaskGroup { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
}