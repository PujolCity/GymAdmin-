namespace GymAdmin.Applications.Interfaces.ValidacionesUI;

public interface IValidationUIService
{
    bool EsTextoValido(string texto, bool permitirNumeros = false);
    bool EsEmailValido(string email);
    bool EsDniValido(string dni);
    bool EsTelefonoValido(string telefono);
    List<string> ValidarNombreCompleto(string texto, string campoNombre);
    string LimpiarYFormatearTexto(string texto);
}