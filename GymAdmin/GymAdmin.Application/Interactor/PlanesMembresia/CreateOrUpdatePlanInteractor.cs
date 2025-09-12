using FluentValidation;
using GymAdmin.Applications.DTOs.MembresiasDto;
using GymAdmin.Applications.Extensions;
using GymAdmin.Applications.Mapppers;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.PlanesMembresia;

public class CreateOrUpdatePlanInteractor : ICreateOrUpdatePlanInteractor
{
    private readonly IPlanMembresiaService _planMembresiaService;
    private readonly IValidator<PlanMembresiaDto> _validator;

    public CreateOrUpdatePlanInteractor(IPlanMembresiaService planMembresiaService, 
        IValidator<PlanMembresiaDto> validator)
    {
        _planMembresiaService = planMembresiaService;
        _validator = validator;
    }

    public async Task<Result> ExecuteAsync(PlanMembresiaDto planDto, CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(planDto, ct);
        if (!validation.IsValid)
            return Result.Fail(validation.Errors.ToErrorMessages());

        if (planDto.Id <= 0)
        {
            var entity = planDto.ToPlanMembresia();
            return await _planMembresiaService.CreateAsync(entity, ct);
        }
        else
        {
            var entity = planDto.ToPlanMembresia();
            return await _planMembresiaService.UpdateAsync(entity, ct);
        }
    }
}
