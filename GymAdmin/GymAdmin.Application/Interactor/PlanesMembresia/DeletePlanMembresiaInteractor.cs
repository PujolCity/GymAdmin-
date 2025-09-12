using FluentValidation;
using GymAdmin.Applications.DTOs;
using GymAdmin.Applications.DTOs.MembresiasDto;
using GymAdmin.Applications.Extensions;
using GymAdmin.Applications.Mapppers;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.PlanesMembresia;

public class DeletePlanMembresiaInteractor : IDeletePlanMembresiaInteractor
{
    private readonly IPlanMembresiaService _planMembresiaService;
    private readonly IValidator<BaseDeleteRequest> _validator;

    public DeletePlanMembresiaInteractor(IPlanMembresiaService planMembresiaService,
        IValidator<BaseDeleteRequest> validator)
    {
        _planMembresiaService = planMembresiaService;
        _validator = validator;
    }

    public async Task<Result> ExecuteAsync(PlanMembresiaDto request, CancellationToken ct = default)
    {
        var requestWrapper = new BaseDeleteRequest { IdToDelete = request.Id };
        var validationResult = await _validator.ValidateAsync(requestWrapper, ct);
       
        if (!validationResult.IsValid)
        {
            return Result.Fail(validationResult.Errors.ToErrorMessages());
        }

        var planToDelete = requestWrapper.ToPlanMembresia();

        return await _planMembresiaService.DeleteAsync(planToDelete, ct);
    }
}
