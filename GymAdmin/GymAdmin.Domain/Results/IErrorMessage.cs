namespace GymAdmin.Domain.Results;


/// <summary>
/// Interfaz para representar un mensaje de error.
/// </summary>
public interface IErrorMessage
{
    string Code { get; set; }

    string Message { get; set; }
}