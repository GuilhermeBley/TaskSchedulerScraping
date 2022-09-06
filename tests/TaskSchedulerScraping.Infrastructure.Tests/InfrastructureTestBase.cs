using Microsoft.Extensions.Configuration;
using TaskSchedulerScraping.Infrastructure.Extensions.IoC;

namespace TaskSchedulerScraping.Infrastructure.Tests;

public class InfrastructureTestBase
{
    protected readonly IServiceProvider _serviceProvider;

    public InfrastructureTestBase()
    {
        var configuration = BuildConfiguration();
        var serviceProvider = BuildServiceProvider(configuration);
        _serviceProvider = serviceProvider;
    }

    private IServiceProvider BuildServiceProvider(IConfiguration configuration)
    {
        IServiceCollection services = new ServiceCollection();

        services.AddInfrastructure(configuration);

        return services.BuildServiceProvider();
    }

    private IConfiguration BuildConfiguration()
    {
        IConfigurationBuilder configurationBuilder =
            new ConfigurationBuilder().AddJsonFile("appsettings.Development.json");

        return configurationBuilder.Build();
    }
}