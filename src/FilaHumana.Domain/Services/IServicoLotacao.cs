using FilaHumana.Domain.Models;

namespace FilaHumana.Domain.Services;

/// <summary>Regras de leitura da fila para o cidadão (lotação e melhor horário).</summary>
public interface IServicoLotacao
{
    /// <summary>Retorna a situação atual do local ou null se o local não existir.</summary>
    Task<LotaçãoAtual?> ObterLotacaoAtualAsync(int localId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calcula, a partir das leituras reais do local, as horas mais vazias
    /// e as mais cheias nos últimos <paramref name="dias"/> dias.
    /// </summary>
    Task<MelhorHorario?> ObterMelhorHorarioAsync(
        int localId,
        int dias = 30,
        CancellationToken cancellationToken = default);
}
