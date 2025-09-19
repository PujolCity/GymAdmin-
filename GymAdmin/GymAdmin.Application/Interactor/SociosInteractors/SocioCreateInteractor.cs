using FluentValidation;
using GymAdmin.Applications.DTOs.SociosDto;
using GymAdmin.Applications.Mapppers;
using GymAdmin.Applications.Extensions;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.SociosInteractors;

public class SocioCreateInteractor : ISocioCreateInteractor
{
    private readonly ISocioService _socioService;
    private readonly IValidator<SocioCreateDto> _validator;

    public SocioCreateInteractor(ISocioService socioService,
        IValidator<SocioCreateDto> validator)
    {
        _socioService = socioService;
        _validator = validator;
    }

    public async Task<Result> ExecuteAsync(SocioCreateDto socioCreateDto)
    {
        var validationResult = await _validator.ValidateAsync(socioCreateDto);
        if (!validationResult.IsValid)
            return Result.Fail(validationResult.Errors.ToErrorMessages());

        var socio = socioCreateDto.ToSocio();
        var result = await _socioService.CreateAsync(socio);
        if (result.IsSuccess)
            return Result.Ok();

        return Result.Fail(result.Errors);
    }
}
