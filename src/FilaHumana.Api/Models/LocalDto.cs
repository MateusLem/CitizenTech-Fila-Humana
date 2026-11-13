using System.ComponentModel.DataAnnotations;

namespace FilaHumana.Api.Models;

/// <summary>Local devolvido pela API.</summary>
public record LocalDto(
    int Id,
    string Nome,
    string Tipo,
    string Bairro,
    string Endereco,
    int CapacidadeConfortavel,
    bool Ativo);

/// <summary>Payload de criação de um local monitorado.</summary>
public class CriarLocalRequest
{
    [Required(ErrorMessage = "Informe o nome do local.")]
    [MaxLength(120)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o tipo do serviço (UBS, Poupatempo...).")]
    [MaxLength(40)]
    public string Tipo { get; set; } = string.Empty;

    [MaxLength(80)]
    public string Bairro { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Endereco { get; set; } = string.Empty;

    [Range(1, 1000, ErrorMessage = "A capacidade confortável deve estar entre 1 e 1000.")]
    public int CapacidadeConfortavel { get; set; }
}
