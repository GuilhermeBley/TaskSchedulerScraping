using TaskSchedulerScraping.Application.Services.Interfaces;

namespace TaskSchedulerScraping.Tests.Infrastructure.Tests;

public class ServiceScrapingTest : InfrastructureTestBase
{
    [Fact]
    public async Task Model_Get_ViewAllModels()
    {
        var scrapingService = 
            _serviceProvider.GetService<IScrapingService>() ?? 
            throw new ArgumentNullException(nameof(IScrapingService));

        var allModels = await scrapingService.GetAllScrapingModelAsync();

        Assert.NotNull(allModels);
    }
}