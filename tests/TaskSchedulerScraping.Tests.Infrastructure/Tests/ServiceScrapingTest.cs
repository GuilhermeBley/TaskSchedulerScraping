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
                Name = nameof(Model_AddAndDeleteWithExecuting_OnException)
            };

        var modelCreate = await _scrapingService.AddScrapingModelAsync(entity);

        Assert.NotNull(modelCreate);

        var modelDeleted = await _scrapingService.DeleteScrapingModelByIdAsync(modelCreate.Id);

        Assert.NotNull(modelDeleted);
    }

    [Fact]
    public async Task Model_AddAndDeleteWithExecuting_OnException()
    {
        var modelCreate = 
            await _scrapingService.AddScrapingModelAsync(
                new Application.Dto.Scraping.ScrapingModelDto()
                {
                    Description = "Test",
                    Name = nameof(Model_AddAndDeleteWithExecuting_OnException)
                }
            );

        Assert.NotNull(modelCreate);

        var executeCreate = 
            await _scrapingService.AddScrapingExecuteAsync(
                new Application.Dto.Scraping.ScrapingExecuteDto()
                {
                    IdScrapingModel = modelCreate.Id,
                    StartAt = DateTime.Now,
                    EndAt = DateTime.Now.AddDays(1),
                }
            );

        await Assert.ThrowsAnyAsync<Application.Exceptions.BadRequestTssException>(()=> _scrapingService.DeleteScrapingModelByIdAsync(modelCreate.Id));

        var modelDeleted = await _scrapingService.DeleteScrapingModelByIdAsync(modelCreate.Id, true);

        Assert.NotNull(modelDeleted);
    }
}