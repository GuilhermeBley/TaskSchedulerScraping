using AutoMapper;
using TaskSchedulerScraping.Application.Dto.Scraping;
using TaskSchedulerScraping.Application.Exceptions;
using TaskSchedulerScraping.Application.Repositories.Scraping;
using TaskSchedulerScraping.Application.Services.Interfaces;
using TaskSchedulerScraping.Application.UoW;
using TaskSchedulerScraping.Domain.Entities.Scraping;

namespace TaskSchedulerScraping.Application.Services.Implementation;

public sealed class ScrapingService : IScrapingService
{
    private readonly IScrapingExecuteRepository _scrapingExecuteRepository;
    private readonly IScrapingModelRepository _scrapingModelRepository;
    private readonly IUnitOfWork _uoW;
    private readonly IMapper _mapper;

    public ScrapingService(
        IScrapingExecuteRepository scrapingExecuteRepository,
        IScrapingModelRepository scrapingModelRepository,
        IUnitOfWork uoW,
        IMapper mapper)
    {
        _scrapingExecuteRepository = scrapingExecuteRepository;
        _scrapingModelRepository = scrapingModelRepository;
        _uoW = uoW;
        _mapper = mapper;
    }

    public async Task<ScrapingExecuteDto> AddScrapingExecuteAsync(ScrapingExecuteDto scrapingExecute)
    {
        var scrapingExecuteMapped = _mapper.Map<ScrapingExecute>(scrapingExecute);

        using (_uoW.BeginTransactionAsync())
        {
            if ((await _scrapingModelRepository.GetByIdOrDefaultAsync(scrapingExecuteMapped.IdScrapingModel)) is null)
                throw new ConflictTssException($"Scraping model with id {scrapingExecuteMapped.IdScrapingModel} don't exists.");

            scrapingExecute =
                (await _scrapingExecuteRepository.AddAsync(scrapingExecuteMapped)) ??
                throw new BadRequestTssException("Failed to insert new Scraping Execute.");

            await _uoW.SaveChangesAsync();
        }

        return _mapper.Map<ScrapingExecuteDto>(scrapingExecuteMapped);
    }

    public async Task<ScrapingModelDto> AddScrapingModelAsync(ScrapingModelDto scrapingModel)
    {
        var scrapingModelMapped = _mapper.Map<ScrapingModel>(scrapingModel);

        using (_uoW.BeginTransactionAsync())
        {
            if ((await _scrapingModelRepository.GetByNameAsync(scrapingModelMapped.NormalizedName)) is not null)
                throw new ConflictTssException($"Scraping model named by {scrapingModelMapped.Name} already exists.");

            scrapingModel =
                (await _scrapingModelRepository.AddAsync(scrapingModelMapped)) ??
                throw new BadRequestTssException("Failed to insert new Scraping Model.");

            await _uoW.SaveChangesAsync();
        }

        return scrapingModel;
    }

    public async Task<IEnumerable<ScrapingExecuteDto>> GetAllScrapingExecuteAsync()
    {
        using (await _uoW.OpenConnectionAsync())
            return await _scrapingExecuteRepository.GetAllAsync();
    }

    public async Task<IEnumerable<ScrapingExecuteDto>> GetAllScrapingExecuteByModelAsync(int idScrapingModel)
    {
        using (await _uoW.OpenConnectionAsync())
            return await _scrapingExecuteRepository.GetAllByModelAsync(idScrapingModel);
    }

    public async Task<IEnumerable<ScrapingModelDto>> GetAllScrapingModelAsync()
    {
        using (await _uoW.OpenConnectionAsync())
            return await _scrapingModelRepository.GetAllAsync();
    }

    public async Task<ScrapingModelDto> GetScrapingModelByNameAsync(string normalizedName)
    {
        using (await _uoW.OpenConnectionAsync())
            return
                (await _scrapingModelRepository.GetByNameAsync(normalizedName)) ??
                throw new NotFoundTssException($"Scraping model named by {normalizedName} could not be found.");
    }
}