using FluentValidation;
using GymAdmin.Applications.DTOs;

namespace GymAdmin.Applications.Validators;

public class BaseDeleteValidator : AbstractValidator<BaseDeleteRequest>
{
    public BaseDeleteValidator()
    {
        RuleFor(x => x.IdToDelete).GreaterThan(0);
    }
}
