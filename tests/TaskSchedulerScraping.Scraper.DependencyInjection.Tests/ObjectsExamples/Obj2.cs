namespace TaskSchedulerScraping.Scraper.DependencyInjection.Tests.ObjectsExamples;

/// <summary>
/// Object type 1
/// </summary>
public class Obj2
{
    private Guid _idInstace { get; } = Guid.NewGuid();
    public Guid IdInstace => _idInstace;
}