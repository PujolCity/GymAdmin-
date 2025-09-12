namespace GymAdmin.Domain.Results;

public class ErrorMessage : IErrorMessage
{
    public string Code { get; set; }

    public string Message { get; set; }

    public ErrorMessage(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public override string ToString()
    {
        return $"{Code} | {Message}";
    }
}