namespace TaskSchedulerScraping.Scraper.DependencyInjection.Tests.Services;

public interface ISimpleService
{
    Guid Id { get; }
}

public class SimpleService : ISimpleService
{
    private Guid _id { get; } = Guid.NewGuid();
    public Guid Id => _id;
}