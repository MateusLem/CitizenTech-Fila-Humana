using FilaHumana.Domain.Enums;

namespace FilaHumana.Domain.Entities;

/// <summary>
/// Uma medição da quantidade de pessoas na fila de um local em um momento.
/// </summary>
public class LeituraDeFila
{
    public int Id { get; private set; }

    public int LocalId { get; private set; }

    public Local? Local { get; private set; }

    /// <summary>Número de pessoas contadas na fila.</summary>
    public int QuantidadePessoas { get; private set; }

    /// <summary>Momento em que a contagem foi registrada (UTC).</summary>
    public DateTime RegistradoEm { get; private set; }

    /// <summary>Como a leitura chegou ao sistema (sensor ESP32, manual...).</summary>
    public OrigemLeitura Origem { get; private set; }

    /// <summary>Uso do Entity Framework Core.</summary>
    private LeituraDeFila()
    {
    }

    public LeituraDeFila(
        int localId,
        int quantidadePessoas,
        DateTime registradoEm,
        OrigemLeitura origem = OrigemLeitura.Sensor)
    {
        if (localId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(localId), "O local da leitura é obrigatório.");
        }

        if (quantidadePessoas < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantidadePessoas),
                "A quantidade de pessoas não pode ser negativa.");
        }

        if (quantidadePessoas > 1_000)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantidadePessoas),
                "Quantidade acima do razoável para uma fila; confira o sensor.");
        }

        if (registradoEm == default)
        {
            throw new ArgumentOutOfRangeException(nameof(registradoEm), "Informe o momento da leitura.");
        }

        LocalId = localId;
        QuantidadePessoas = quantidadePessoas;
        RegistradoEm = registradoEm;
        Origem = origem;
    }
}
