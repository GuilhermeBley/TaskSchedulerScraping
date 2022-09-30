using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TaskSchedulerScraping.Scraper.DependencyInjection.Tests;

/// <summary>
/// Test base with service providier
/// </summary>
public class ServicesTestBase
{
    /// <summary>
    /// Services builded
    /// </summary>
    protected readonly IServiceProvider _serviceProvider;

    /// <inheritdoc cref="_serviceProvider" path="*"/>
    public IServiceProvider ServiceProvider => _serviceProvider;

    public ServicesTestBase(Action<IServiceCollection>? actionServices = null)
    {
        var configuration = BuildConfiguration();
        var serviceProvider = BuildServiceProvider(configuration, actionServices);
        _serviceProvider = serviceProvider;
    }

    private IServiceProvider BuildServiceProvider(IConfiguration configuration, Action<IServiceCollection>? actionServices = null)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(configuration);
        actionServices?.Invoke(services);
        return services.BuildServiceProvider();
    }

    private IConfiguration BuildConfiguration()
    {
        IConfigurationBuilder configurationBuilder =
            new ConfigurationBuilder().AddJsonFile("appsettings.Development.json");

        return configurationBuilder.Build();
    }
}