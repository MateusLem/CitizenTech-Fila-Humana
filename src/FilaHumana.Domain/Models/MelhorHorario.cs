namespace FilaHumana.Domain.Models;

/// <summary>Média de pessoas na fila em uma faixa horária.</summary>
public record ResumoHorario(int Hora, double MediaPessoas, int Amostras);

/// <summary>
/// Resultado do cálculo de melhor horário: as horas mais vazias e as mais cheias
/// do período analisado, calculadas com LINQ sobre as leituras reais do local.
/// </summary>
public record MelhorHorario(
    int LocalId,
    string Nome,
    int DiasAnalisados,
    IReadOnlyList<ResumoHorario> MelhoresHorarios,
    IReadOnlyList<ResumoHorario> HorariosPico);
