using System.ComponentModel.DataAnnotations;
using FilaHumana.Domain.Enums;

namespace FilaHumana.Api.Models;

/// <summary>Payload enviado pelo circuito ESP32 (ou por testes manuais) a cada contagem.</summary>
public class RegistrarLeituraRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Informe o local da leitura.")]
    public int LocalId { get; set; }

    [Range(0, 1000, ErrorMessage = "A quantidade deve estar entre 0 e 1000.")]
    public int QuantidadePessoas { get; set; }

    /// <summary>Momento da contagem em UTC. Se vazio, a API usa o horário do servidor.</summary>
    public DateTime? RegistradoEm { get; set; }

    public OrigemLeitura Origem { get; set; } = OrigemLeitura.Sensor;
}

/// <summary>Leitura devolvida pela API.</summary>
public record LeituraDto(
    int Id,
    int LocalId,
    int QuantidadePessoas,
    DateTime RegistradoEm,
    string Origem);
