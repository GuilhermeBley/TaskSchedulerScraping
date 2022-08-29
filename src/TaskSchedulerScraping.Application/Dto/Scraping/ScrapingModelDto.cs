namespace TaskSchedulerScraping.Application.Dto.Scraping;

public class ScrapingModelDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
}