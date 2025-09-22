using FluentValidation;
using GymAdmin.Applications.DTOs.Asistencia;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.AsistenciaInteractors;

public class UpdateAsistenciaInteractor : IUpdateAsistenciaInteractor
{
    private readonly IAsistenciaService _asistenciaService;
    private readonly IValidator<AsistenciaDto> _validator;

    public UpdateAsistenciaInteractor(IAsistenciaService asistenciaService,
        IValidator<AsistenciaDto> validator)
    {
        _asistenciaService = asistenciaService;
        _validator = validator;
    }

    public async Task<Result> ExecuteAsync(AsistenciaDto asistenciaDto, CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(asistenciaDto, ct);
        if (!validation.IsValid)
            return Result.Fail(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var asistencia = new Asistencia
        {
            Id = asistenciaDto.Id,
            SocioId = asistenciaDto.SocioId,
            Entrada = asistenciaDto.Entrada,
            Observaciones = asistenciaDto.Observaciones
        };

        return await _asistenciaService.UpdateAsync(asistencia, ct);
    }
}
