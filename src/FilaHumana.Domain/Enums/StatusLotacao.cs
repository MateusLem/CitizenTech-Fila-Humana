namespace FilaHumana.Domain.Enums;

/// <summary>
/// Situação da fila de um local, calculada a partir da quantidade de pessoas
/// e da capacidade confortável definida para o local.
/// </summary>
public enum StatusLotacao
{
    /// <summary>Menos de 50% da capacidade confortável.</summary>
    Vazio = 0,

    /// <summary>Entre 50% e 100% da capacidade confortável.</summary>
    Confortavel = 1,

    /// <summary>Entre 100% e 150% da capacidade confortável.</summary>
    Movimentado = 2,

    /// <summary>Acima de 150% da capacidade confortável.</summary>
    Lotado = 3
}
