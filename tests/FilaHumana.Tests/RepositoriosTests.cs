using FilaHumana.Domain.Entities;
using FilaHumana.Domain.Enums;
using FilaHumana.Infrastructure.Dados;
using FilaHumana.Infrastructure.Repositorios;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FilaHumana.Tests;

/// <summary>
/// Testes de integração dos repositórios com SQLite em memória —
/// comprovam a conexão com o banco e as consultas LINQ.
/// </summary>
public class RepositoriosTests : IDisposable
{
    private readonly SqliteConnection _conexao;
    private readonly FilaHumanaContext _db;
    private readonly RepositorioLocais _locais;
    private readonly RepositorioLeituras _leituras;

    public RepositoriosTests()
    {
        _conexao = new SqliteConnection("Filename=:memory:");
        _conexao.Open();

        var opcoes = new DbContextOptionsBuilder<FilaHumanaContext>()
            .UseSqlite(_conexao)
            .Options;

        _db = new FilaHumanaContext(opcoes);
        _db.Database.EnsureCreated();

        _locais = new RepositorioLocais(_db);
        _leituras = new RepositorioLeituras(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
        _conexao.Dispose();
    }

    [Fact]
    public async Task Local_e_salvo_e_recuperado_com_leituras()
    {
        var local = await _locais.AdicionarAsync(
            new Local("UBS Teste Integração", "UBS", "Centro", "Rua A, 10", 8));

        await _leituras.AdicionarAsync(new LeituraDeFila(local.Id, 4, DateTime.UtcNow));

        var recuperado = await _locais.ObterPorIdAsync(local.Id);

        Assert.NotNull(recuperado);
        Assert.Equal("UBS Teste Integração", recuperado!.Nome);
        Assert.Equal(8, recuperado.CapacidadeConfortavel);
        Assert.True(await _locais.ExisteAsync(local.Id));
    }

    [Fact]
    public async Task Listar_por_local_filtro_o_periodo_e_devolve_em_ordem_cronologica()
    {
        var local = await _locais.AdicionarAsync(
            new Local("UBS Período", "UBS", "Centro", "Rua A, 10", 10));

        var base_ = new DateTime(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc);
        await _leituras.AdicionarAsync(new LeituraDeFila(local.Id, 5, base_));
        await _leituras.AdicionarAsync(new LeituraDeFila(local.Id, 7, base_.AddHours(2)));
        await _leituras.AdicionarAsync(new LeituraDeFila(local.Id, 3, base_.AddHours(4)));
        // Fora do período pedido:
        await _leituras.AdicionarAsync(new LeituraDeFila(local.Id, 99, base_.AddDays(10)));

        var doPeriodo = await _leituras.ListarPorLocalAsync(
            local.Id,
            base_.AddHours(-1),
            base_.AddHours(6));

        Assert.Equal(3, doPeriodo.Count);
        Assert.Equal(new[] { 5, 7, 3 }, doPeriodo.Select(l => l.QuantidadePessoas).ToArray());
        Assert.True(doPeriodo[0].RegistradoEm < doPeriodo[^1].RegistradoEm);
    }

    [Fact]
    public async Task Ultima_por_local_retorna_a_leitura_mais_recente()
    {
        var local = await _locais.AdicionarAsync(
            new Local("UBS Última", "UBS", "Centro", "Rua A, 10", 10));

        var agora = DateTime.UtcNow;
        await _leituras.AdicionarAsync(new LeituraDeFila(local.Id, 6, agora.AddHours(-3)));
        await _leituras.AdicionarAsync(new LeituraDeFila(local.Id, 9, agora));
        await _leituras.AdicionarAsync(new LeituraDeFila(local.Id, 2, agora.AddHours(-1)));

        var ultima = await _leituras.UltimaPorLocalAsync(local.Id);

        Assert.NotNull(ultima);
        Assert.Equal(9, ultima!.QuantidadePessoas);
        Assert.Equal(agora, ultima.RegistradoEm);
    }

    [Fact]
    public async Task Leitura_com_origem_sensor_e_persistida()
    {
        var local = await _locais.AdicionarAsync(
            new Local("UBS Sensor", "UBS", "Centro", "Rua A, 10", 10));

        var criada = await _leituras.AdicionarAsync(
            new LeituraDeFila(local.Id, 11, DateTime.UtcNow, OrigemLeitura.Sensor));

        var recuperada = await _leituras.UltimaPorLocalAsync(local.Id);

        Assert.Equal(criada.Id, recuperada!.Id);
        Assert.Equal(OrigemLeitura.Sensor, recuperada.Origem);
    }
}
