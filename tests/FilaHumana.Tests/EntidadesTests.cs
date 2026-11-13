using FilaHumana.Domain.Entities;
using FilaHumana.Domain.Enums;

namespace FilaHumana.Tests;

/// <summary>Regras de negócio das entidades (POO) — testadas sem infraestrutura.</summary>
public class EntidadesTests
{
    [Theory]
    [InlineData(0, StatusLotacao.Vazio)]
    [InlineData(5, StatusLotacao.Vazio)]      // 5/12 = 41%  → < 50%
    [InlineData(7, StatusLotacao.Confortavel)] // 7/12 = 58%  → < 100%
    [InlineData(11, StatusLotacao.Confortavel)]// 11/12 = 91% → < 100%
    [InlineData(12, StatusLotacao.Movimentado)]// 100%        → < 150%
    [InlineData(17, StatusLotacao.Movimentado)]// 141%        → < 150%
    [InlineData(18, StatusLotacao.Lotado)]     // 150%        → >= 150%
    public void Local_AvaliarStatus_respeita_os_limites(int quantidade, StatusLotacao esperado)
    {
        var local = new Local("UBS Teste", "UBS", "Bairro", "Rua X, 1", capacidadeConfortavel: 12);

        var status = local.AvaliarStatus(quantidade);

        Assert.Equal(esperado, status);
    }

    [Fact]
    public void Local_com_capacidade_zerada_e_invalido()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Local("UBS Teste", "UBS", "Bairro", "Rua X, 1", capacidadeConfortavel: 0));
    }

    [Fact]
    public void Local_sem_nome_e_invalido()
    {
        Assert.Throws<ArgumentException>(() =>
            new Local("  ", "UBS", "Bairro", "Rua X, 1", 10));
    }

    [Fact]
    public void Leitura_com_quantidade_negativa_e_invalida()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LeituraDeFila(1, -1, DateTime.UtcNow));
    }

    [Fact]
    public void Leitura_acima_do_limite_do_sensor_e_invalida()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LeituraDeFila(1, 5_000, DateTime.UtcNow));
    }

    [Fact]
    public void Leitura_guarda_local_quantidade_momento_e_origem()
    {
        var momento = new DateTime(2026, 11, 10, 9, 0, 0, DateTimeKind.Utc);

        var leitura = new LeituraDeFila(7, 9, momento, OrigemLeitura.Sensor);

        Assert.Equal(7, leitura.LocalId);
        Assert.Equal(9, leitura.QuantidadePessoas);
        Assert.Equal(momento, leitura.RegistradoEm);
        Assert.Equal(OrigemLeitura.Sensor, leitura.Origem);
    }

    [Fact]
    public void Usuario_rejeita_email_invalido()
    {
        Assert.Throws<ArgumentException>(() => new Usuario("Maria", "email-sem-arroba"));
    }
}
