using TaskSchedulerScraping.Domain.Validations;

namespace TaskSchedulerScraping.Domain.Entities.Scraping;

public class ScrapingModel
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public string? Description { get; private set; } = string.Empty;

    private ScrapingModel(
        int id,
        string name,
        string normalizedName,
        string? description)
    {
        Id = id;
        Name = name;
        NormalizedName = normalizedName;
        Description = description;
    }

    public static IValidationResult<ScrapingModel> Create(
        int id,
        string name,
        string? description)
    {
        var execeptions = new List<string>();

        if (id < 0)
            execeptions.Add($"{nameof(id)} must be greater than '0'.");

        if (string.IsNullOrEmpty(name))
            execeptions.Add($"Has null or empty required strings.");

        if (!name.All(char.IsLetter))
            execeptions.Add($"{nameof(name)} must be have only letters.");

        if (execeptions.Any())
            return ValidationResult<ScrapingModel>.GetWithErrors(execeptions);

        return ValidationResult<ScrapingModel>.GetSuccess(
            new ScrapingModel(id, name, name.ToUpper(), description)
        );
    }
}