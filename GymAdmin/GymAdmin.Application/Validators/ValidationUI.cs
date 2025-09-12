using GymAdmin.Applications.Interfaces.ValidacionesUI;
using System.Text.RegularExpressions;

namespace GymAdmin.Applications.Validators;

public class ValidationUIService : IValidationUIService
{
    // Regex compiladas para mejor performance
    private static readonly Regex _textoRegex = new Regex(@"^[\p{L}\s'-´`]+$", RegexOptions.Compiled);
    private static readonly Regex _textoConNumerosRegex = new Regex(@"^[\p{L}\s'-´`\d]+$", RegexOptions.Compiled);
    private static readonly Regex _emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex _soloDigitosRegex = new Regex(@"^\d+$", RegexOptions.Compiled);

    public bool EsTextoValido(string texto, bool permitirNumeros = false)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return false;

        var regex = permitirNumeros ? _textoConNumerosRegex : _textoRegex;
        return regex.IsMatch(texto);
    }

    public bool EsEmailValido(string email)
    {
        return !string.IsNullOrWhiteSpace(email) && _emailRegex.IsMatch(email);
    }

    public bool EsDniValido(string dni)
    {
        return !string.IsNullOrWhiteSpace(dni) &&
               dni.Length == 8 &&
               _soloDigitosRegex.IsMatch(dni);
    }

    public bool EsTelefonoValido(string telefono)
    {
        return !string.IsNullOrWhiteSpace(telefono) &&
               telefono.Length >= 6 &&
               _soloDigitosRegex.IsMatch(telefono);
    }

    public List<string> ValidarNombreCompleto(string texto, string campoNombre)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(texto))
        {
            errores.Add($"{campoNombre} es requerido");
            return errores;
        }

        if (!EsTextoValido(texto))
        {
            errores.Add($"El {campoNombre} solo puede contener letras, espacios y los caracteres: - ' ´ `");
        }

        if (texto.Trim() != texto)
        {
            errores.Add($"El {campoNombre} no debe tener espacios al inicio o final");
        }

        if (texto.Contains("  "))
        {
            errores.Add($"El {campoNombre} no debe tener espacios múltiples consecutivos");
        }

        if (texto.Trim().Length < 2)
        {
            errores.Add($"El {campoNombre} debe tener al menos 2 caracteres");
        }

        return errores;
    }

    public string LimpiarYFormatearTexto(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return texto;

        // Trim y eliminar espacios múltiples
        return Regex.Replace(texto.Trim(), @"\s+", " ");
    }
}