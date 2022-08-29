using TaskSchedulerScraping.Domain.Validations;

namespace TaskSchedulerScraping.Domain.Entities.Scraping;

public class ScrapingExecute
{
    public int Id { get; private set; }
    public int IdScrapingModel { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime? EndAt { get; private set; }

    public ScrapingExecute(
        int id,
        int idScrapingModel,
        DateTime startAt,
        DateTime? endAt)
    {
        Id = id;
        IdScrapingModel = idScrapingModel;
        StartAt = startAt;
        EndAt = endAt;
    }

    public static IValidationResult<ScrapingExecute> Create(
        int id,
        int idScrapingModel,
        DateTime startAt,
        DateTime? endAt)
    {
        var execeptions = new List<string>();

        if (id < 0)
            execeptions.Add($"{nameof(id)} must be greater or equals than '0'.");

        if (idScrapingModel < 1)
            execeptions.Add($"{nameof(id)} must be greater than '0'.");

        if (startAt >= DateTime.Now)
            execeptions.Add($"{nameof(startAt)} must be greater or equals than {DateTime.Now}.");

        if (endAt is not null &&  
			(endAt <= DateTime.Now || endAt.Equals(DateTime.MinValue))
			)
            execeptions.Add($"{nameof(endAt)} must be less  or equals than {DateTime.Now}.");

        if (idScrapingModel < 1)
            execeptions.Add($"{nameof(idScrapingModel)} must be grater than '0'.");

        if (execeptions.Any())
            return ValidationResult<ScrapingExecute>.GetWithErrors(execeptions);

        return ValidationResult<ScrapingExecute>.GetSuccess(
            new ScrapingExecute(id, idScrapingModel, startAt, endAt)
        );
    }
}