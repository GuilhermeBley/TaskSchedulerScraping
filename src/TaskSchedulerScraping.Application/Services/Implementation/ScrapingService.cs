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

        return scrapingExecute;
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

    public async Task<ScrapingExecuteDto> DeleteScrapingExecuteByIdAsync(int idScrapingExecute)
    {
        ScrapingExecuteDto scrapingExecuteDto;

        using (await _uoW.BeginTransactionAsync())
        {
            scrapingExecuteDto = await WithOutConnDeleteScrapingExecuteByIdAsync(idScrapingExecute);

            await _uoW.SaveChangesAsync();
        }

        return scrapingExecuteDto;
    }

    public async Task<ScrapingModelDto> DeleteScrapingModelByIdAsync(int idScrapingModel, bool confirmDeleteRelationalExecute = false)
    {
        ScrapingModelDto scrapingModel;

        using (_uoW.BeginTransactionAsync())
        {
            scrapingModel =
                (await _scrapingModelRepository.GetByIdOrDefaultAsync(idScrapingModel)) ??
                throw new NotFoundTssException($"Scraping model with id {idScrapingModel} could not be found.");

            var scrapingExecutions = await _scrapingExecuteRepository.GetAllByModelAsync(scrapingModel.Id);

            if (scrapingExecutions.Any() && !confirmDeleteRelationalExecute)
                throw new BadRequestTssException($"Has related data in Scraping Execute with Scraping Model '{scrapingModel.Name}'," +
                " please add a confirmation relatioships deletion or delete manually.");

            foreach (var scrapingExecute in scrapingExecutions)
            {
                await WithOutConnDeleteScrapingExecuteByIdAsync(scrapingExecute.Id);
            }

            if ((await _scrapingModelRepository.DeleteByIdAsync(scrapingModel.Id)) is null)
                throw new BadRequestTssException("Failed to delete scraping model.");

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

    public async Task<ScrapingModelDto?> GetScrapingModelByNameAsync(string normalizedName)
    {
        using (await _uoW.OpenConnectionAsync())
            return
                await _scrapingModelRepository.GetByNameAsync(normalizedName);
    }

    private async Task<ScrapingExecuteDto> WithOutConnDeleteScrapingExecuteByIdAsync(int idScrapingExecute)
    {
        ScrapingExecuteDto scrapingExecute =
            (await _scrapingExecuteRepository.GetByIdOrDefaultAsync(idScrapingExecute)) ??
            throw new NotFoundTssException($"Scraping execute with id {idScrapingExecute} could not be found.");

        return
            (await _scrapingExecuteRepository.DeleteByIdAsync(scrapingExecute.Id)) ??
            throw new BadRequestTssException("Failed to delete Scraping Execute.");
    }
}