using FilaHumana.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FilaHumana.Infrastructure.Dados;

/// <summary>
/// Mapeamento Fluent API das entidades e seed estático (locais e usuários fictícios)
/// que acompanha a migração. As leituras de fila são geradas em tempo de execução
/// por <see cref="DadosDeDemonstracao"/> para que o demo funcione em qualquer data.
/// </summary>
public static class ConfiguracoesDoModelo
{
    public static void ConfigurarEntidades(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Local>(entidade =>
        {
            entidade.ToTable("Locais");
            entidade.HasKey(local => local.Id);
            entidade.Property(local => local.Nome).IsRequired().HasMaxLength(120);
            entidade.Property(local => local.Tipo).IsRequired().HasMaxLength(40);
            entidade.Property(local => local.Bairro).IsRequired().HasMaxLength(80);
            entidade.Property(local => local.Endereco).IsRequired().HasMaxLength(200);
            entidade.Property(local => local.CapacidadeConfortavel).IsRequired();
            entidade.HasIndex(local => local.Bairro);

            entidade
                .HasMany(local => local.Leituras)
                .WithOne(leitura => leitura.Local!)
                .HasForeignKey(leitura => leitura.LocalId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LeituraDeFila>(entidade =>
        {
            entidade.ToTable("LeiturasDeFila");
            entidade.HasKey(leitura => leitura.Id);
            entidade.Property(leitura => leitura.QuantidadePessoas).IsRequired();
            entidade.Property(leitura => leitura.Origem).HasConversion<string>().HasMaxLength(20);
            entidade.HasIndex(leitura => new { leitura.LocalId, leitura.RegistradoEm });
        });

        modelBuilder.Entity<Usuario>(entidade =>
        {
            entidade.ToTable("Usuarios");
            entidade.HasKey(usuario => usuario.Id);
            entidade.Property(usuario => usuario.Nome).IsRequired().HasMaxLength(120);
            entidade.Property(usuario => usuario.Email).IsRequired().HasMaxLength(200);
            entidade.HasIndex(usuario => usuario.Email).IsUnique();
        });
    }

    /// <summary>
    /// Dados estáticos fictícios gravados pela migração.
    /// Nenhum dado pessoal real é usado no projeto.
    /// Os IDs são explícitos porque o EF exige chave própria nos dados de seed.
    /// </summary>
    public static void SemearDadosEstaticos(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Local>().HasData(
            new { Id = 1, Nome = "UBS Vila Mariana", Tipo = "UBS", Bairro = "Vila Mariana", Endereco = "Rua das Palmeiras, 100", CapacidadeConfortavel = 12, Ativo = true },
            new { Id = 2, Nome = "UBS Butantã", Tipo = "UBS", Bairro = "Butantã", Endereco = "Av. Professor Alfonso Bovero, 500", CapacidadeConfortavel = 10, Ativo = true },
            new { Id = 3, Nome = "Poupatempo Sé", Tipo = "Poupatempo", Bairro = "Sé", Endereco = "Praça da Sé, s/n", CapacidadeConfortavel = 20, Ativo = true },
            new { Id = 4, Nome = "Poupatempo Tatuapé", Tipo = "Poupatempo", Bairro = "Tatuapé", Endereco = "Av. Celso Garcia, 1500", CapacidadeConfortavel = 16, Ativo = true });

        var momentoSeed = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Usuario>().HasData(
            new { Id = 1, Nome = "Cidadão de Demonstração", Email = "demo@filahumana.test", ReceberAlerta = true, CadastradoEm = momentoSeed },
            new { Id = 2, Nome = "Usuária de Teste", Email = "teste@filahumana.test", ReceberAlerta = false, CadastradoEm = momentoSeed });
    }
}
