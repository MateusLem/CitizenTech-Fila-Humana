using FilaHumana.Domain.Abstractions;
using FilaHumana.Domain.Entities;
using FilaHumana.Domain.Enums;
using FilaHumana.Domain.Models;
using FilaHumana.Domain.Services;
using Moq;

namespace FilaHumana.Tests;

/// <summary>
/// Testes do serviço de lotação com repositórios mockados (Moq) —
/// validam a regra de negócio sem depender do banco.
/// </summary>
public class ServicoLotacaoTests
{
    private readonly Local _local = new("UBS Teste", "UBS", "Bairro", "Rua X, 1", 10);

    private readonly Mock<IRepositorioLocais> _locais = new();
    private readonly Mock<IRepositorioLeituras> _leituras = new();

    private ServicoLotacao CriarServico() => new(_locais.Object, _leituras.Object);

    [Fact]
    public async Task Lotacao_atual_usa_a_ultima_leitura_e_calcula_o_status()
    {
        _locais.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_local);
        _leituras.Setup(r => r.UltimaPorLocalAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LeituraDeFila(1, 12, DateTime.UtcNow));

        var resultado = await CriarServico().ObterLotacaoAtualAsync(1);

        Assert.NotNull(resultado);
        Assert.Equal(12, resultado!.QuantidadePessoas);
        Assert.Equal(StatusLotacao.Movimentado, resultado.Status); // 12/10 = 120%
        Assert.Equal("UBS Teste", resultado.Nome);
    }

    [Fact]
    public async Task Lotacao_atual_sem_leituras_zera_a_quantidade()
    {
        _locais.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_local);
        _leituras.Setup(r => r.UltimaPorLocalAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeituraDeFila?)null);

        var resultado = await CriarServico().ObterLotacaoAtualAsync(1);

        Assert.NotNull(resultado);
        Assert.Equal(0, resultado!.QuantidadePessoas);
        Assert.Equal(StatusLotacao.Vazio, resultado.Status);
        Assert.Null(resultado.RegistradoEm);
    }

    [Fact]
    public async Task Lotacao_atual_devolve_null_para_local_inexistente()
    {
        _locais.Setup(r => r.ObterPorIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Local?)null);

        var resultado = await CriarServico().ObterLotacaoAtualAsync(99);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task Melhor_horario_retorna_as_horas_mais_vazias_em_ordem_crescente()
    {
        _locais.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_local);

        var agora = DateTime.UtcNow;
        var leituras = new List<LeituraDeFila>();

        // 9h sempre cheia (15 pessoas), 14h sempre vazia (2), 10h intermediária (8).
        for (var dias = 1; dias <= 10; dias++)
        {
            var dia = agora.AddDays(-dias);
            leituras.Add(new LeituraDeFila(1, 15, dia.Date.AddHours(9)));
            leituras.Add(new LeituraDeFila(1, 8, dia.Date.AddHours(10)));
            leituras.Add(new LeituraDeFila(1, 2, dia.Date.AddHours(14)));
        }

        _leituras.Setup(r => r.ListarPorLocalAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(leituras);

        var resultado = await CriarServico().ObterMelhorHorarioAsync(1, dias: 30);

        Assert.NotNull(resultado);
        Assert.Equal(30, resultado!.DiasAnalisados);
        Assert.Equal(14, resultado.MelhoresHorarios[0].Hora);
        Assert.Equal(10, resultado.MelhoresHorarios[1].Hora);
        Assert.Equal(9, resultado.MelhoresHorarios[2].Hora);

        // Pico vem do lado oposto: 9h é a pior.
        Assert.Equal(9, resultado.HorariosPico[0].Hora);
        Assert.Equal(10, resultado.HorariosPico[1].Hora);
        Assert.Equal(14, resultado.HorariosPico[2].Hora);

        Assert.All(resultado.MelhoresHorarios, r => Assert.Equal(10, r.Amostras));
    }

    [Fact]
    public async Task Melhor_horario_devolve_null_para_local_inexistente()
    {
        _locais.Setup(r => r.ObterPorIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Local?)null);

        var resultado = await CriarServico().ObterMelhorHorarioAsync(42);

        Assert.Null(resultado);
    }
}
