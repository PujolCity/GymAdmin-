using FluentValidation;
using GymAdmin.Applications.DTOs;
using GymAdmin.Applications.DTOs.SociosDto;
using GymAdmin.Applications.Extensions;
using GymAdmin.Applications.Mapppers;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.SociosInteractors;

public class DeleteSocioInteractor : IDeleteSocioInteractor
{
    private readonly ISocioService _socioService;
    private readonly IValidator<BaseDeleteRequest> _validator;

    public DeleteSocioInteractor(ISocioService socioService, 
        IValidator<BaseDeleteRequest> validator)
    {
        _socioService = socioService;
        _validator = validator;
    }

    public async Task<Result> ExecuteAsync(SocioDto socioDto, CancellationToken ct = default)
    {
        var requestWrapper = new BaseDeleteRequest { IdToDelete = socioDto.Id };
        var validationResult = await _validator.ValidateAsync(requestWrapper, ct);

        if (!validationResult.IsValid)
        {
            return Result.Fail(validationResult.Errors.ToErrorMessages());
        }

        var socioToDelete = requestWrapper.ToSocio();
        return await _socioService.DeleteAsync(socioToDelete, ct);
    }
}
