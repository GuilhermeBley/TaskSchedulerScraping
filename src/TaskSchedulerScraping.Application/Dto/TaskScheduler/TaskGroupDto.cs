namespace TaskSchedulerScraping.Application.Dto.TaskScheduler;

public class TaskGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public DateTime CreateAt { get; set; }
}