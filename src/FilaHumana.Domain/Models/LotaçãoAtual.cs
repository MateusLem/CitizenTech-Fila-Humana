using FilaHumana.Domain.Enums;

namespace FilaHumana.Domain.Models;

/// <summary>Lotação atual de um local, pronta para exibição ao cidadão.</summary>
public record LotaçãoAtual(
    int LocalId,
    string Nome,
    string Tipo,
    string Bairro,
    int QuantidadePessoas,
    StatusLotacao Status,
    DateTime? RegistradoEm,
    OrigemLeitura? Origem);
