using FluentValidation;
using GymAdmin.Applications.DTOs.ConfiguracionDto;

namespace GymAdmin.Applications.Validators;

public class RestoreBackupDtoValidator : AbstractValidator<RestoreBackupDto>
{
    public RestoreBackupDtoValidator()
    {
        RuleFor(x => x.ZipFilePath)
         .NotEmpty()
         .WithMessage("Debe seleccionar un archivo ZIP.")
         .Must(File.Exists)
         .WithMessage("El archivo seleccionado no existe.")
         .Must(path => string.Equals(
             Path.GetExtension(path),
             ".zip",
             StringComparison.OrdinalIgnoreCase))
         .WithMessage("El archivo seleccionado no es un archivo ZIP válido.");
    }
}
