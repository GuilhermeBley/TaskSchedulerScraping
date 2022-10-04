namespace TaskSchedulerScraping.Scraper.DependencyInjection.Tests.Services;

/// <summary>
/// Service linked with <see cref="ISimpleService"/>
/// </summary>
public interface IServiceLinkedWithOther
{
    Guid Id { get; }
    ISimpleService SimpleService { get; }
}

/// <inheritdoc path="*"/>
public class ServiceLinkedWithOther : IServiceLinkedWithOther
{
    private Guid _id { get; } = Guid.NewGuid();
    public Guid Id => _id;
    private ISimpleService _linkedService;
    public ISimpleService SimpleService => _linkedService;

    /// <summary>
    /// Inject other service
    /// </summary>
    public ServiceLinkedWithOther(ISimpleService service)
    {
        _linkedService = service;
    }
}