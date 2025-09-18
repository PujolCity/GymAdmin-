using FluentValidation;
using GymAdmin.Applications.DTOs.Asistencia;
using GymAdmin.Applications.Extensions;
using GymAdmin.Applications.Mapppers;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.AsistenciaInteractors;

public class CreateAsistenciaInteractor : ICreateAsistenciaInteractor
{
    private readonly ISocioService _socioService;
    private readonly IValidator<CreateAsistenciaDto> _validator;

    public CreateAsistenciaInteractor(ISocioService socioService, 
        IValidator<CreateAsistenciaDto> validator)
    {
        _socioService = socioService;
        _validator = validator;
    }

    public async Task<Result> ExecuteAsync(CreateAsistenciaDto asistenciaDto, CancellationToken ct = default)
    {
        var validationResult = _validator.Validate(asistenciaDto);
        if (!validationResult.IsValid)
            return Result.Fail(validationResult.Errors.ToErrorMessages());

        var asistencia = asistenciaDto.ToAsistencia();

        var result = await _socioService.RegistrarAsistenciaAsync(asistencia, ct);

        if (result.IsSuccess)
            return Result.Ok();

        return Result.Fail(result.Errors);
    }
}
