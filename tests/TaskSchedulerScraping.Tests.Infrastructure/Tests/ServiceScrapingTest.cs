using TaskSchedulerScraping.Application.Services.Interfaces;

namespace TaskSchedulerScraping.Tests.Infrastructure.Tests;

public class ServiceScrapingTest : InfrastructureTestBase
{
    private readonly IScrapingService _scrapingService;

    public ServiceScrapingTest()
    {
        _scrapingService = _serviceProvider.GetService<IScrapingService>() ?? 
            throw new ArgumentNullException(nameof(IScrapingService));
    }

    [Fact]
    public async Task Model_Get_ViewAllModels()
    {
        var allModels = await _scrapingService.GetAllScrapingModelAsync();

        Assert.NotNull(allModels);
    }

    [Fact]
    public async Task Model_AddAndDelete_AddAndDeleteModel()
    {
        var entity =
            new Application.Dto.Scraping.ScrapingModelDto()
            {
                Description = "Test",
                Name = "Test"
            };

        var modelCreate = await _scrapingService.AddScrapingModelAsync(entity);

        Assert.NotNull(modelCreate);

        var modelDeleted = await _scrapingService.DeleteScrapingModelByIdAsync(modelCreate.Id);

        Assert.NotNull(modelDeleted);
    }
}