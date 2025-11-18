namespace GymAdmin.Domain.Interfaces.Bussiness;

public interface ICalculadorTipoAjuste
{
    decimal Aplicar(decimal montoBase, decimal valor);
}
