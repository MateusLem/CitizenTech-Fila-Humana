using FilaHumana.Domain.Abstractions;
using FilaHumana.Domain.Entities;
using FilaHumana.Infrastructure.Dados;
using Microsoft.EntityFrameworkCore;

namespace FilaHumana.Infrastructure.Repositorios;

/// <summary>Implementação assíncrona do repositório de leituras de fila com EF Core.</summary>
public class RepositorioLeituras : IRepositorioLeituras
{
    private readonly FilaHumanaContext _db;

    public RepositorioLeituras(FilaHumanaContext db)
    {
        _db = db;
    }

    public async Task<LeituraDeFila> AdicionarAsync(
        LeituraDeFila leitura,
        CancellationToken cancellationToken = default)
    {
        _db.Leituras.Add(leitura);
        await _db.SaveChangesAsync(cancellationToken);
        return leitura;
    }

    public async Task<IReadOnlyList<LeituraDeFila>> ListarPorLocalAsync(
        int localId,
        DateTime desde,
        DateTime ate,
        CancellationToken cancellationToken = default)
    {
        return await _db
            .Leituras
            .AsNoTracking()
            .Where(leitura =>
                leitura.LocalId == localId &&
                leitura.RegistradoEm >= desde &&
                leitura.RegistradoEm <= ate)
            .OrderBy(leitura => leitura.RegistradoEm)
            .ToListAsync(cancellationToken);
    }

    public async Task<LeituraDeFila?> UltimaPorLocalAsync(
        int localId,
        CancellationToken cancellationToken = default)
    {
        return await _db
            .Leituras
            .AsNoTracking()
            .Where(leitura => leitura.LocalId == localId)
            .OrderByDescending(leitura => leitura.RegistradoEm)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
