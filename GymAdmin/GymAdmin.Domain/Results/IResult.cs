namespace GymAdmin.Domain.Results;

/// <summary>
/// Interfaz base para resultados.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public interface IResult<out TValue> : IResultBase
{
    TValue Value { get; }
}