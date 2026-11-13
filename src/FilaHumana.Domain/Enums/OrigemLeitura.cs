namespace FilaHumana.Domain.Enums;

/// <summary>De onde veio a leitura de fila.</summary>
public enum OrigemLeitura
{
    /// <summary>Contagem automática enviada pelo circuito ESP32 (sensor PIR/ultrassônico).</summary>
    Sensor = 0,

    /// <summary>Leitura inserida manualmente pela equipe em testes.</summary>
    Manual = 1,

    /// <summary>Leitura vindade de outra integração (API externa, importação).</summary>
    Integracao = 2
}
