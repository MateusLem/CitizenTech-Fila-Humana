using FilaHumana.Domain.Entities;

namespace FilaHumana.Domain.Abstractions;

/// <summary>Contrato de acesso às leituras de fila — implementado na camada Infrastructure.</summary>
public interface IRepositorioLeituras
{
    Task<LeituraDeFila> AdicionarAsync(LeituraDeFila leitura, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeituraDeFila>> ListarPorLocalAsync(
        int localId,
        DateTime desde,
        DateTime ate,
        CancellationToken cancellationToken = default);

    Task<LeituraDeFila?> UltimaPorLocalAsync(int localId, CancellationToken cancellationToken = default);
}
