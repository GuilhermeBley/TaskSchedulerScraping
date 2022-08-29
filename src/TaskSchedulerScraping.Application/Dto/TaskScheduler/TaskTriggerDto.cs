namespace TaskSchedulerScraping.Application.Dto.TaskScheduler;
public class TaskTriggerDto
{
    public int Id { get; set; }
    public int IdTaskRegistration { get; set; } = 0;
    public DateTime UpdateAt { get; set; }
    public bool Enabled { get; set; }
    public DateTime Start { get; set; }
    public int IdTaskOnSchedule { get; set; } = 0;
    public DateTime? Expire { get; set; }
}