namespace GymAdmin.Domain.Interfaces.Bussiness;

public interface ICalculadorTipoAjuste
{
    decimal Calcular(decimal montoBase, decimal valor);
    decimal CalcularDelta(decimal baseAmount, decimal valorAjuste);
}
