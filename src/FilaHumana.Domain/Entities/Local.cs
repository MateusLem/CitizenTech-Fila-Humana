using FilaHumana.Domain.Enums;

namespace FilaHumana.Domain.Entities;

/// <summary>
/// Local de atendimento público monitorado pela Fila Humana
/// (ex.: Unidade Básica de Saúde, Poupatempo).
/// </summary>
public class Local
{
    public int Id { get; private set; }

    /// <summary>Nome exibido ao cidadão (ex.: "UBS Vila Mariana").</summary>
    public string Nome { get; private set; } = string.Empty;

    /// <summary>Tipo do serviço (ex.: "UBS", "Poupatempo").</summary>
    public string Tipo { get; private set; } = string.Empty;

    public string Bairro { get; private set; } = string.Empty;

    public string Endereco { get; private set; } = string.Empty;

    /// <summary>
    /// Quantidade de pessoas na fila considerada confortável.
    /// É a base do cálculo de <see cref="StatusLotacao"/>.
    /// </summary>
    public int CapacidadeConfortavel { get; private set; }

    /// <summary>Locais inativos saem da listagem do app, mas mantêm o histórico.</summary>
    public bool Ativo { get; private set; } = true;

    public ICollection<LeituraDeFila> Leituras { get; private set; } = new List<LeituraDeFila>();

    /// <summary>Uso do Entity Framework Core.</summary>
    private Local()
    {
    }

    public Local(string nome, string tipo, string bairro, string endereco, int capacidadeConfortavel)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("O nome do local é obrigatório.", nameof(nome));
        }

        if (string.IsNullOrWhiteSpace(tipo))
        {
            throw new ArgumentException("O tipo do local é obrigatório.", nameof(tipo));
        }

        if (capacidadeConfortavel <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacidadeConfortavel),
                "A capacidade confortável deve ser maior que zero.");
        }

        Nome = nome.Trim();
        Tipo = tipo.Trim();
        Bairro = bairro?.Trim() ?? string.Empty;
        Endereco = endereco?.Trim() ?? string.Empty;
        CapacidadeConfortavel = capacidadeConfortavel;
    }

    /// <summary>
    /// Regra de negócio central do projeto: transforma uma contagem de pessoas
    /// em um status simples que qualquer cidadão entende.
    /// </summary>
    public StatusLotacao AvaliarStatus(int quantidadePessoas)
    {
        if (quantidadePessoas < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantidadePessoas),
                "A quantidade de pessoas não pode ser negativa.");
        }

        var fator = (double)quantidadePessoas / CapacidadeConfortavel;

        return fator switch
        {
            < 0.5 => StatusLotacao.Vazio,
            < 1.0 => StatusLotacao.Confortavel,
            < 1.5 => StatusLotacao.Movimentado,
            _ => StatusLotacao.Lotado
        };
    }
}
