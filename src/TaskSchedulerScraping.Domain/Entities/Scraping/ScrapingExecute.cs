using TaskSchedulerScraping.Domain.Validations;

namespace TaskSchedulerScraping.Domain.Entities.Scraping;

public class ScrapingExecute
{
    public int Id { get; private set; }
    public int IdScrapingModel { get; private set; }
    public ScrapingModel ScrapingModel { get; private set; } = null!;
    public DateTime StartAt { get; private set; }
    public DateTime? EndAt { get; private set; }

    public ScrapingExecute(
        int id,
        int idScrapingModel,
        ScrapingModel scrapingModel,
        DateTime startAt,
        DateTime? endAt)
    {
        Id = id;
        IdScrapingModel = idScrapingModel;
        ScrapingModel = scrapingModel;
        StartAt = startAt;
        EndAt = endAt;
    }

    public static IValidationResult<ScrapingExecute> Create(
        int id,
        int idScrapingModel,
        ScrapingModel scrapingModel,
        DateTime startAt,
        DateTime? endAt)
    {
        var execeptions = new List<string>();

        if (id < 0)
            execeptions.Add($"{nameof(id)} must be greater than '0'.");

        if (startAt >= DateTime.Now)
            execeptions.Add($"{nameof(startAt)} must be greater or equals than {DateTime.Now}.");

        if (endAt is not null &&  
			(endAt <= DateTime.Now || endAt.Equals(DateTime.MinValue))
			)
            execeptions.Add($"{nameof(endAt)} must be less  or equals than {DateTime.Now}.");

        if (scrapingModel is null || idScrapingModel < 1)
            execeptions.Add($"{nameof(scrapingModel)} is null.");

        if (execeptions.Any())
            return ValidationResult<ScrapingExecute>.GetWithErrors(execeptions);

        return ValidationResult<ScrapingExecute>.GetSuccess(
            new ScrapingExecute(id, idScrapingModel, scrapingModel!, startAt, endAt)
        );
    }
}