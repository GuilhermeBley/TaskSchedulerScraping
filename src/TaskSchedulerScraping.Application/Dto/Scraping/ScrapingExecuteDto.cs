namespace TaskSchedulerScraping.Application.Dto.Scraping;

public class ScrapingExecuteDto
{
    public int Id { get; set; } = 0;
    public int IdScrapingModel { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
}