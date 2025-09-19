using GymAdmin.Applications.DTOs.PagosDto;
using GymAdmin.Applications.Mapppers;
using GymAdmin.Domain.Interfaces.Services;
using System.Globalization;
using System.Text;

namespace GymAdmin.Applications.Interactor.PagosInteractors;

public class GetSociosLookupInteractor : IGetSociosLookupInteractor
{
    private readonly ISocioService _socioService;

    public GetSociosLookupInteractor(ISocioService socioService)
    {
        _socioService = socioService;
    }

    public async Task<List<SocioLookupDto>> ExecuteAsync(string texto, CancellationToken ct = default)
    {
        var socios = await _socioService.GetAllForLookupAsync(ct);

        if (string.IsNullOrWhiteSpace(texto))
        {
            return socios
                .Take(20)
                .Select(s => s.ToLookupDto())
                .ToList();
        }

        var q = Normalize(texto);

        var res = socios
            .Where(s =>
            {
                var nombreComp = Normalize($"{s.Nombre} {s.Apellido}");
                return nombreComp.Contains(q)
                       || (!string.IsNullOrEmpty(s.Dni) && s.Dni.Contains(texto));
            })
            .OrderBy(s => s.Apellido)
            .ThenBy(s => s.Nombre)
            .Take(20) 
            .Select(s => s.ToLookupDto())
            .ToList();

        return res;
    }

    private static string Normalize(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var formD = input.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(capacity: formD.Length);
        foreach (var ch in formD)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
