using Microsoft.Extensions.DependencyInjection;
using TaskSchedulerScraping.Infrastructure.Connection;
using TaskSchedulerScraping.Infrastructure.UoW;
using TaskSchedulerScraping.Application.UoW;
using Microsoft.Extensions.Configuration;

namespace TaskSchedulerScraping.Infrastructure.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSingleton<IConfiguration>(configuration)
            .AddTransient<IConnectionFactory, ConnectionFactory>()
            .AddScoped<UnitOfWorkRepository>()
            .AddScoped<IUnitOfWorkRepository>(x => x.GetRequiredService<UnitOfWorkRepository>())
            .AddScoped<IUnitOfWork>(x => x.GetRequiredService<UnitOfWorkRepository>());

        return services;
    }
}