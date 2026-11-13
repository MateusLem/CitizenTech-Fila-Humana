using FilaHumana.Domain.Abstractions;
using FilaHumana.Domain.Models;

namespace FilaHumana.Domain.Services;

/// <summary>
/// Implementação das regras de leitura da fila. Depende apenas das abstrações
/// de repositório, nunca do Entity Framework — o que permite testar com mocks.
/// </summary>
public class ServicoLotacao : IServicoLotacao
{
    private const int HorariosRetornados = 3;
    private readonly IRepositorioLocais _locais;
    private readonly IRepositorioLeituras _leituras;

    public ServicoLotacao(IRepositorioLocais locais, IRepositorioLeituras leituras)
    {
        _locais = locais;
        _leituras = leituras;
    }

    public async Task<LotaçãoAtual?> ObterLotacaoAtualAsync(
        int localId,
        CancellationToken cancellationToken = default)
    {
        var local = await _locais.ObterPorIdAsync(localId, cancellationToken);
        if (local is null)
        {
            return null;
        }

        var ultima = await _leituras.UltimaPorLocalAsync(localId, cancellationToken);
        var quantidade = ultima?.QuantidadePessoas ?? 0;

        return new LotaçãoAtual(
            local.Id,
            local.Nome,
            local.Tipo,
            local.Bairro,
            quantidade,
            local.AvaliarStatus(quantidade),
            ultima?.RegistradoEm,
            ultima?.Origem);
    }

    public async Task<MelhorHorario?> ObterMelhorHorarioAsync(
        int localId,
        int dias = 30,
        CancellationToken cancellationToken = default)
    {
        var local = await _locais.ObterPorIdAsync(localId, cancellationToken);
        if (local is null)
        {
            return null;
        }

        var periodo = Math.Clamp(dias, 1, 365);
        var agora = DateTime.UtcNow;
        var leituras = await _leituras.ListarPorLocalAsync(
            localId,
            agora.AddDays(-periodo),
            agora,
            cancellationToken);

        var porHora = leituras
            .GroupBy(leitura => leitura.RegistradoEm.Hour)
            .Select(grupo => new ResumoHorario(
                grupo.Key,
                Math.Round(grupo.Average(leitura => leitura.QuantidadePessoas), 1),
                grupo.Count()))
            .OrderBy(resumo => resumo.MediaPessoas)
            .ThenBy(resumo => resumo.Hora)
            .ToList();

        var melhores = porHora.Take(HorariosRetornados).ToList();
        var pico = porHora.TakeLast(HorariosRetornados).Reverse().ToList();

        return new MelhorHorario(local.Id, local.Nome, periodo, melhores, pico);
    }
}
