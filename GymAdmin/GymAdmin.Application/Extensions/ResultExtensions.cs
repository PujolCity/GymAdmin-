using FluentValidation.Results;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Extensions;

/// <summary>
/// Extensiones para convertir resultados de validación en resultados de dominio.
/// </summary>
public static class ResultExtensions
{
    public static Result ToResult(this ValidationResult validationResult)
    {
        if (validationResult.IsValid)
            return Result.Ok();

        var errors = validationResult.Errors
            .Select(e => new ErrorMessage(e.PropertyName, e.ErrorMessage));
        return Result.Fail(errors);
    }
    public static Result<T> ToResult<T>(this ValidationResult validationResult, T value = default)
    {
        if (validationResult.IsValid)
            return Result<T>.Ok(value);

        var errors = validationResult.Errors
            .Select(e => new ErrorMessage(e.PropertyName, e.ErrorMessage));
        return Result<T>.Fail(errors);
    }

    public static List<IErrorMessage> ToErrorMessages(this IEnumerable<ValidationFailure> failures)
    {
        return [.. failures
            .Select(e => new ErrorMessage(string.Concat("ValidationError-", e.PropertyName), e.ErrorMessage))
            .Cast<IErrorMessage>()];
    }
}
