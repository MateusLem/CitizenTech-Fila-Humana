using FilaHumana.Domain.Entities;
using FilaHumana.Infrastructure.Dados;
using Microsoft.EntityFrameworkCore;

namespace FilaHumana.Infrastructure.Dados;

/// <summary>
/// Contexto do Entity Framework Core — única dependência de persistência do projeto.
/// O mapeamento (Fluent API) e o seed estático ficam em <see cref="ConfiguracoesDoModelo"/>.
/// </summary>
public class FilaHumanaContext : DbContext
{
    public FilaHumanaContext(DbContextOptions<FilaHumanaContext> options)
        : base(options)
    {
    }

    public DbSet<Local> Locais => Set<Local>();

    public DbSet<LeituraDeFila> Leituras => Set<LeituraDeFila>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ConfigurarEntidades();
        modelBuilder.SemearDadosEstaticos();
    }
}
