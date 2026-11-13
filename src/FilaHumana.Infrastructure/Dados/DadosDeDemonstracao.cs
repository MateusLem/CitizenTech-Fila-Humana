using FilaHumana.Domain.Entities;
using FilaHumana.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FilaHumana.Infrastructure.Dados;

/// <summary>
/// Gera as leituras de demonstração na primeira execução, quando a tabela está vazia.
/// O padrão imita o comportamento real de uma fila (pico de manhã e início da tarde,
/// menos movimento aos fins de semana) para que o app e o cálculo de melhor horário
/// tenham o que exibir.
/// ATENÇÃO: são dados fictícios de demonstração — não usar na entrega de IA & ML,
/// que exige apenas dados reais e públicos.
/// </summary>
public static class DadosDeDemonstracao
{
    private static readonly int[] HorarioBase =
    {
        /* 00 */ 0, 0, 0, 0, 0, 0, 0,
        /* 07 */ 4, 9, 14, 12, 8,
        /* 12 */ 6, 10, 13, 11, 7,
        /* 17 */ 5, 3, 2, 0, 0, 0, 0
    };

    public static async Task GarantirLeiturasAsync(FilaHumanaContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Leituras.AnyAsync(cancellationToken))
        {
            return;
        }

        var locais = await db.Locais.AsNoTracking().ToListAsync(cancellationToken);
        if (locais.Count == 0)
        {
            return;
        }

        var aleatorio = new Random(42);
        var agora = DateTime.UtcNow;
        var novasLeituras = new List<LeituraDeFila>();

        foreach (var local in locais)
        {
            // Fator de demanda próprio de cada unidade, derivado da capacidade cadastrada.
            var fatorLocal = local.CapacidadeConfortavel / 14.0;

            for (var diasAtras = 13; diasAtras >= 0; diasAtras--)
            {
                var dia = agora.Date.AddDays(-diasAtras);
                var fimDeSemana = dia.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
                var fatorDia = fimDeSemana ? 0.4 : 1.0;

                for (var hora = 7; hora <= 19; hora++)
                {
                    // Não cria leituras no futuro.
                    if (diasAtras == 0 && hora > agora.Hour)
                    {
                        continue;
                    }

                    var baseDia = HorarioBase[hora] * fatorDia * fatorLocal;
                    var variacao = aleatorio.Next(-2, 3);
                    var quantidade = Math.Clamp((int)Math.Round(baseDia + variacao), 0, local.CapacidadeConfortavel * 2);
                    var momento = new DateTime(dia.Year, dia.Month, dia.Day, hora, 0, 0, DateTimeKind.Utc);

                    novasLeituras.Add(new LeituraDeFila(
                        local.Id,
                        quantidade,
                        momento,
                        OrigemLeitura.Sensor));
                }
            }
        }

        db.Leituras.AddRange(novasLeituras);
        await db.SaveChangesAsync(cancellationToken);
    }
}
