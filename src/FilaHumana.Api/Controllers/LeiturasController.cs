using FilaHumana.Api.Models;
using FilaHumana.Domain.Abstractions;
using FilaHumana.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FilaHumana.Api.Controllers;

/// <summary>
/// Endpoints das leituras de fila: recebimento da contagem vinda do ESP32
/// e consulta do histórico por período.
/// </summary>
[ApiController]
[Route("api/leituras")]
[Produces("application/json")]
public class LeiturasController : ControllerBase
{
    private readonly IRepositorioLeituras _leituras;
    private readonly IRepositorioLocais _locais;

    public LeiturasController(IRepositorioLeituras leituras, IRepositorioLocais locais)
    {
        _leituras = leituras;
        _locais = locais;
    }

    /// <summary>
    /// Registra uma nova contagem de fila — endpoint chamado pelo circuito ESP32
    /// a cada leitura do sensor (HTTP POST com Wi-Fi).
    /// </summary>
    /// <response code="201">Leitura registrada.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Local inexistente.</response>
    [HttpPost]
    [ProducesResponseType<LeituraDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeituraDto>> Registrar(
        [FromBody] RegistrarLeituraRequest requisicao,
        CancellationToken cancellationToken)
    {
        if (!await _locais.ExisteAsync(requisicao.LocalId, cancellationToken))
        {
            return NotFound(new { mensagem = $"Local {requisicao.LocalId} não encontrado." });
        }

        try
        {
            var leitura = new LeituraDeFila(
                requisicao.LocalId,
                requisicao.QuantidadePessoas,
                requisicao.RegistradoEm ?? DateTime.UtcNow,
                requisicao.Origem);

            var criada = await _leituras.AdicionarAsync(leitura, cancellationToken);

            var dto = new LeituraDto(
                criada.Id,
                criada.LocalId,
                criada.QuantidadePessoas,
                criada.RegistradoEm,
                criada.Origem.ToString());

            return CreatedAtAction(nameof(Listar), new { localId = criada.LocalId }, dto);
        }
        catch (ArgumentOutOfRangeException erro)
        {
            return BadRequest(new { mensagem = erro.Message });
        }
    }

    /// <summary>Consulta o histórico de leituras de um local em um período.</summary>
    /// <response code="200">Lista de leituras ordenada por horário.</response>
    /// <response code="400">Período inválido.</response>
    /// <response code="404">Local inexistente.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<LeituraDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<LeituraDto>>> Listar(
        [FromQuery] int localId,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? ate = null,
        CancellationToken cancellationToken = default)
    {
        if (!await _locais.ExisteAsync(localId, cancellationToken))
        {
            return NotFound(new { mensagem = $"Local {localId} não encontrado." });
        }

        var inicio = desde ?? DateTime.UtcNow.AddDays(-7);
        var fim = ate ?? DateTime.UtcNow;

        if (fim < inicio)
        {
            return BadRequest(new { mensagem = "O período final deve ser posterior ao inicial." });
        }

        var leituras = await _leituras.ListarPorLocalAsync(localId, inicio, fim, cancellationToken);

        var resposta = leituras
            .Select(leitura => new LeituraDto(
                leitura.Id,
                leitura.LocalId,
                leitura.QuantidadePessoas,
                leitura.RegistradoEm,
                leitura.Origem.ToString()))
            .ToList();

        return Ok(resposta);
    }
}
