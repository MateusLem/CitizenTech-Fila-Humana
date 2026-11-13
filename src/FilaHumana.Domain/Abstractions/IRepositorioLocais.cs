using FilaHumana.Domain.Entities;

namespace FilaHumana.Domain.Abstractions;

/// <summary>Contrato de acesso aos locais — implementado na camada Infrastructure.</summary>
public interface IRepositorioLocais
{
    Task<IReadOnlyList<Local>> ListarAsync(bool apenasAtivos = true, CancellationToken cancellationToken = default);

    Task<Local?> ObterPorIdAsync(int localId, CancellationToken cancellationToken = default);

    Task<Local> AdicionarAsync(Local local, CancellationToken cancellationToken = default);

    Task<bool> ExisteAsync(int localId, CancellationToken cancellationToken = default);
}
