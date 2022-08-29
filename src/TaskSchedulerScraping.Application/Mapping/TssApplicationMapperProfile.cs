using AutoMapper;
using TaskSchedulerScraping.Application.Dto.Scraping;
using TaskSchedulerScraping.Application.Dto.TaskScheduler;
using TaskSchedulerScraping.Application.Exceptions;
using TaskSchedulerScraping.Domain.Entities.Scraping;
using TaskSchedulerScraping.Domain.Entities.TaskScheduler;
using TaskSchedulerScraping.Domain.Validations;

namespace TaskSchedulerScraping.Application.Mapping;

public class TssApplicationMapperProfile : Profile
{
    public TssApplicationMapperProfile()
    {
        CreateMap<ScrapingExecute, ScrapingExecuteDto>()
            .ReverseMap()
            .ConstructUsing(map =>
                GetValidation(
                    ScrapingExecute.Create(map.Id, map.IdScrapingModel, map.StartAt, map.EndAt)
                )
            );
        CreateMap<ScrapingModel, ScrapingModelDto>()
            .ReverseMap()
            .ConstructUsing(map =>
                GetValidation(
                    ScrapingModel.Create(map.Id, map.Name, map.Description)
                )
            );

        CreateMap<TaskAction, TaskActionDto>()
            .ReverseMap()
            .ConstructUsing(map =>
                GetValidation(
                    TaskAction.Create(map.Id, map.IdTaskRegistration, map.UpdateAt, map.ProgramOrScript, map.Args, map.StartIn)
                )
            );
        CreateMap<TaskGroup, TaskGroupDto>()
            .ReverseMap()
            .ConstructUsing(map =>
                GetValidation(
                    TaskGroup.Create(map.Id, map.Name, map.CreateAt)
                )
            );
        CreateMap<TaskOnSchedule, TaskOnScheduleDto>()
            .ReverseMap()
            .ConstructUsing(map =>
                GetValidation(
                    TaskOnSchedule.Create(map.Id, map.Name)
                )
            );
        CreateMap<TaskRegistration, TaskRegistrationDto>()
            .ReverseMap()
            .ConstructUsing(dto =>
                GetValidation(
                    TaskRegistration.Create(dto.Id, dto.IdTaskGroup, dto.Name, dto.Location, dto.Name, dto.Author)
                )
            );
        CreateMap<TaskTrigger, TaskTriggerDto>()
            .ReverseMap()
            .ConstructUsing(dto =>
                GetValidation<TaskTrigger>(
                    TaskTrigger.Create(dto.Id, dto.IdTaskRegistration, dto.UpdateAt, dto.Enabled, dto.Start, dto.IdTaskOnSchedule, dto.Expire)
                )
            );
    }

    private static T GetValidation<T>(IValidationResult<T> validation)
        where T : class
    {
        if (!validation.IsSuccess)
            throw new InvalidModelTssException(validation.Errors);

        return validation.Result!;
    }
}