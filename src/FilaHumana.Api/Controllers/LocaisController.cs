using FilaHumana.Api.Models;
using FilaHumana.Domain.Abstractions;
using FilaHumana.Domain.Entities;
using FilaHumana.Domain.Models;
using FilaHumana.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace FilaHumana.Api.Controllers;

/// <summary>
/// Endpoints de locais: listagem, criação, situação atual da fila
/// e melhor horário para ir (o coração da Fila Humana).
/// </summary>
[ApiController]
[Route("api/locais")]
[Produces("application/json")]
public class LocaisController : ControllerBase
{
    private readonly IRepositorioLocais _locais;
    private readonly IServicoLotacao _servicoLotacao;

    public LocaisController(IRepositorioLocais locais, IServicoLotacao servicoLotacao)
    {
        _locais = locais;
        _servicoLotacao = servicoLotacao;
    }

    /// <summary>Lista os locais monitorados pela Fila Humana.</summary>
    /// <response code="200">Lista de locais.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<LocalDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LocalDto>>> Listar(CancellationToken cancellationToken)
    {
        var locais = await _locais.ListarAsync(apenasAtivos: true, cancellationToken);

        var resposta = locais
            .Select(local => new LocalDto(
                local.Id,
                local.Nome,
                local.Tipo,
                local.Bairro,
                local.Endereco,
                local.CapacidadeConfortavel,
                local.Ativo))
            .ToList();

        return Ok(resposta);
    }

    /// <summary>Retorna um local pelo identificador.</summary>
    /// <response code="200">Local encontrado.</response>
    /// <response code="404">Local inexistente.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<LocalDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocalDto>> Obter(int id, CancellationToken cancellationToken)
    {
        var local = await _locais.ObterPorIdAsync(id, cancellationToken);
        if (local is null)
        {
            return NotFound(new { mensagem = $"Local {id} não encontrado." });
        }

        return Ok(new LocalDto(
            local.Id,
            local.Nome,
            local.Tipo,
            local.Bairro,
            local.Endereco,
            local.CapacidadeConfortavel,
            local.Ativo));
    }

    /// <summary>Cadastra um novo local monitorado.</summary>
    /// <response code="201">Local criado.</response>
    /// <response code="400">Dados inválidos.</response>
    [HttpPost]
    [ProducesResponseType<LocalDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LocalDto>> Criar(
        [FromBody] CriarLocalRequest requisicao,
        CancellationToken cancellationToken)
    {
        try
        {
            var local = new Local(
                requisicao.Nome,
                requisicao.Tipo,
                requisicao.Bairro,
                requisicao.Endereco,
                requisicao.CapacidadeConfortavel);

            var criado = await _locais.AdicionarAsync(local, cancellationToken);
            var dto = new LocalDto(
                criado.Id,
                criado.Nome,
                criado.Tipo,
                criado.Bairro,
                criado.Endereco,
                criado.CapacidadeConfortavel,
                criado.Ativo);

            return CreatedAtAction(nameof(Obter), new { id = criado.Id }, dto);
        }
        catch (ArgumentException erro)
        {
            return BadRequest(new { mensagem = erro.Message });
        }
    }

    /// <summary>
    /// Situação atual da fila do local (quantidade de pessoas e status
    /// Vazio/Confortável/Movimentado/Lotado) — é o que o app exibe.
    /// </summary>
    /// <response code="200">Lotação atual.</response>
    /// <response code="404">Local inexistente.</response>
    [HttpGet("{id:int}/lotacao")]
    [ProducesResponseType<LotaçãoAtual>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LotaçãoAtual>> ObterLotacao(int id, CancellationToken cancellationToken)
    {
        var lotacao = await _servicoLotacao.ObterLotacaoAtualAsync(id, cancellationToken);
        if (lotacao is null)
        {
            return NotFound(new { mensagem = $"Local {id} não encontrado." });
        }

        return Ok(lotacao);
    }

    /// <summary>
    /// Calcula, com as leituras reais do local, os horários mais vazios e os de pico
    /// nos últimos N dias — a resposta para "qual o melhor horário para ir".
    /// </summary>
    /// <param name="id">Identificador do local.</param>
    /// <param name="dias">Período analisado (1 a 365 dias, padrão 30).</param>
    /// <param name="cancellationToken">Token de cancelamento da operação assíncrona.</param>
    /// <response code="200">Melhores horários e horários de pico.</response>
    /// <response code="404">Local inexistente.</response>
    [HttpGet("{id:int}/melhor-horario")]
    [ProducesResponseType<MelhorHorario>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MelhorHorario>> ObterMelhorHorario(
        int id,
        [FromQuery] int dias = 30,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _servicoLotacao.ObterMelhorHorarioAsync(id, dias, cancellationToken);
        if (resultado is null)
        {
            return NotFound(new { mensagem = $"Local {id} não encontrado." });
        }

        return Ok(resultado);
    }
}
