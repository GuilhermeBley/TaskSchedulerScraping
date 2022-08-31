using Microsoft.Extensions.DependencyInjection;
using TaskSchedulerScraping.Application.UoW;
using TaskSchedulerScraping.Application.Repositories.TaskScheduler;
using TaskSchedulerScraping.Infrastructure.Connection;
using TaskSchedulerScraping.Infrastructure.Repositories.TaskScheduler;
using TaskSchedulerScraping.Infrastructure.UoW;
using Microsoft.Extensions.Configuration;
using TaskSchedulerScraping.Application.Repositories.Scraping;
using TaskSchedulerScraping.Infrastructure.Repositories.Scraping;
using TaskSchedulerScraping.Application.Services.Implementation;
using TaskSchedulerScraping.Application.Services.Interfaces;

namespace TaskSchedulerScraping.Infrastructure.Extensions.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        #region Configuration
        services
            .AddSingleton<IConfiguration>(configuration);
        #endregion
        
        #region  Connection MySql
        services
            .AddTransient<IConnectionFactory, ConnectionFactory>()
            .AddScoped<UnitOfWorkRepository>()
            .AddScoped<IUnitOfWorkRepository>(x => x.GetRequiredService<UnitOfWorkRepository>())
            .AddScoped<IUnitOfWork>(x => x.GetRequiredService<UnitOfWorkRepository>());
        #endregion

        #region  Repositories
        services
            .AddScoped<IScrapingExecuteRepository, ScrapingExecuteRepository>()
            .AddScoped<IScrapingModelRepository, ScrapingModelRepository>()
            .AddScoped<ITaskActionRepository, TaskActionRepository>()
            .AddScoped<ITaskGroupRepository, TaskGroupRepository>()
            .AddScoped<ITaskOnScheduleRepository, TaskOnScheduleRepository>()
            .AddScoped<ITaskRegistrationRepository, TaskRegistrationRepository>()
            .AddScoped<ITaskTriggerRepository, TaskTriggerRepository>();
        #endregion

        #region  Services
        services
            .AddScoped<IScrapingService, ScrapingService>()
            .AddScoped<ITaskSchedulerService, TaskSchedulerService>();
        #endregion
        
        #region  AutoMapper
        services
            .AddAutoMapper(
                typeof(TaskSchedulerScraping.Application.Mapping.TssApplicationMapperProfile)
            );
        #endregion

        return services;
    }
}