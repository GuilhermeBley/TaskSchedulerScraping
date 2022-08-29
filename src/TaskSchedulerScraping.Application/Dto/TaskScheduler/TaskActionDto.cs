namespace TaskSchedulerScraping.Application.Dto.TaskScheduler;

public class TaskActionDto
{
    public int Id { get; set; } = 0;
    public int IdTaskRegistration { get; set; }
    public DateTime UpdateAt { get; set; }
    public string ProgramOrScript { get; set; } = string.Empty;
    public string Args { get; set; } = string.Empty;
    public string StartIn { get; set; } = string.Empty;
}