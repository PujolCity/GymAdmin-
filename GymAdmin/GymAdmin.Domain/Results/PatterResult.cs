using FluentValidation.Results;

namespace GymAdmin.Domain.Results;

/// <summary>
/// Interfaz base para resultados.
/// </summary>
public interface IResultBase
{
    bool IsSuccess { get; }
    bool IsFailed { get; }
    List<IErrorMessage> Errors { get; }
}

public abstract class ResultBase : IResultBase
{
    public bool IsSuccess => !IsFailed;
    public bool IsFailed => Errors.Count != 0;
    public List<IErrorMessage> Errors { get; }

    protected ResultBase()
    {
        Errors = new List<IErrorMessage>();
    }
}

public abstract class ResultBase<TResult> : ResultBase where TResult : ResultBase<TResult>
{
    public TResult WithError(IErrorMessage error)
    {
        Errors.Add(error);
        return (TResult)this;
    }

    public TResult WithErrors(IEnumerable<IErrorMessage> errors)
    {
        Errors.AddRange(errors);
        return (TResult)this;
    }

    public TResult WithError(string message) => WithError(new ErrorMessage("", message));
}


public class Result : ResultBase<Result>
{
    public static Result Ok() => new Result();

    public static Result Fail(IErrorMessage error) => new Result().WithError(error);

    public static Result Fail(string message) => Fail(new ErrorMessage("", message));

    public static Result Fail(IEnumerable<IErrorMessage> errors) => new Result().WithErrors(errors);

    public static Result Fail(IEnumerable<ValidationFailure> validationErrors)
    {
        var errors = validationErrors.Select(e => new ErrorMessage(e.PropertyName, e.ErrorMessage));
        return Fail(errors);
    }
}

public class Result<TValue> : ResultBase<Result<TValue>>, IResult<TValue>
{
    private readonly TValue? _value;

    public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException(
        $"Cannot access Value of failed result. Errors: {string.Join(", ", Errors)}");

    private Result(TValue value)
    {
        _value = value;
    }

    private Result(IEnumerable<IErrorMessage> errors)
    {
        WithErrors(errors);
    }

    public static Result<TValue> Ok(TValue value) => new(value);

    public static Result<TValue> Fail(IErrorMessage error) => new([error]);

    public static Result<TValue> Fail(string message) => Fail(new ErrorMessage("", message));

    public static Result<TValue> Fail(IEnumerable<IErrorMessage> errors) => new(errors);

    public static Result<TValue> Fail(IEnumerable<ValidationFailure> validationErrors)
    {
        var errors = validationErrors.Select(e => new ErrorMessage(e.PropertyName, e.ErrorMessage));
        return Fail(errors);
    }
}