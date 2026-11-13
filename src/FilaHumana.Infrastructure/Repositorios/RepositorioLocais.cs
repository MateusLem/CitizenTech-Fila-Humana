using FilaHumana.Domain.Abstractions;
using FilaHumana.Domain.Entities;
using FilaHumana.Infrastructure.Dados;
using Microsoft.EntityFrameworkCore;

namespace FilaHumana.Infrastructure.Repositorios;

/// <summary>Implementação assíncrona do repositório de locais com EF Core + LINQ.</summary>
public class RepositorioLocais : IRepositorioLocais
{
    private readonly FilaHumanaContext _db;

    public RepositorioLocais(FilaHumanaContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Local>> ListarAsync(
        bool apenasAtivos = true,
        CancellationToken cancellationToken = default)
    {
        return await _db
            .Locais
            .AsNoTracking()
            .Where(local => !apenasAtivos || local.Ativo)
            .OrderBy(local => local.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<Local?> ObterPorIdAsync(int localId, CancellationToken cancellationToken = default)
    {
        return await _db
            .Locais
            .AsNoTracking()
            .FirstOrDefaultAsync(local => local.Id == localId, cancellationToken);
    }

    public async Task<Local> AdicionarAsync(Local local, CancellationToken cancellationToken = default)
    {
        _db.Locais.Add(local);
        await _db.SaveChangesAsync(cancellationToken);
        return local;
    }

    public async Task<bool> ExisteAsync(int localId, CancellationToken cancellationToken = default)
    {
        return await _db.Locais.AnyAsync(local => local.Id == localId, cancellationToken);
    }
}
