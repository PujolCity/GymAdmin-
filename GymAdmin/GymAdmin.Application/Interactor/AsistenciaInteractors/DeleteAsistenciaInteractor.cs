using FluentValidation;
using GymAdmin.Applications.DTOs.Asistencia;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.AsistenciaInteractors;

public class DeleteAsistenciaInteractor : IDeleteAsistenciaInteractor
{
    private readonly IAsistenciaService _asistenciaService;
    private readonly IValidator<DeleteAsistenciaDto> _validator;

    public DeleteAsistenciaInteractor(IAsistenciaService asistenciaService, 
        IValidator<DeleteAsistenciaDto> validator)
    {
        _asistenciaService = asistenciaService;
        _validator = validator;
    }

    public async Task<Result> ExecuteAsync(DeleteAsistenciaDto request, CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Result.Fail(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var asistencia = new Asistencia
        {
            Id = request.Id,
            SocioId = request.SocioId
        };

        return await _asistenciaService.DeleteAsync(asistencia, ct);
    }
}
