using FluentValidation;
using GymAdmin.Applications.DTOs.SociosDto;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.SociosInteractors;

public class UpdateSocioInteractor : IUpdateSocioInteractor
{
    private readonly ISocioService _socioService;
    private readonly IValidator<SocioUpdateDto> _validator;

    public UpdateSocioInteractor(ISocioService socioService, 
        IValidator<SocioUpdateDto> validator)
    {
        _socioService = socioService;
        _validator = validator;
    }

    public async Task<Result> ExecuteAsync(SocioUpdateDto socioDto, CancellationToken cancellation = default)
    {
        if (socioDto is null)
            return Result.Fail("El socio no puede ser nulo.");
        
        var validationResult = await _validator.ValidateAsync(socioDto, cancellation);
        if (!validationResult.IsValid)
            return Result.Fail(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var socio = new Socio
        {
            Id = socioDto.Id,
            Nombre = socioDto.Nombre,
            Apellido = socioDto.Apellido,
            Dni = socioDto.Dni,
            IsActive = socioDto.IsActive,
            Telefono = socioDto.Telefono
        };

        return await _socioService.UpdateAsync(socio, cancellation);
    }
}
