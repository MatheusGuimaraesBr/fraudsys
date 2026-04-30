using FraudSys.Application.DTOs;
using FraudSys.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FraudSys.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LimiteContaController : ControllerBase
{
    private readonly LimiteContaService _service;

    public LimiteContaController(LimiteContaService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Cadastrar([FromBody] CadastrarLimiteRequest request)
    {
        try
        {
            await _service.CadastrarAsync(request);
            return Created("", new { mensagem = "Limite cadastrado com sucesso." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

[HttpGet("{cpf}/{conta}")]
public async Task<IActionResult> Buscar(string cpf, string conta)
{
    try
    {
        var resultado = await _service.BuscarAsync(cpf, conta);

        if (resultado == null)
            return NotFound(new { erro = "Conta não encontrada." });

        return Ok(resultado);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { erro = ex.Message });
    }
}

[HttpPut("{cpf}/{conta}")]
public async Task<IActionResult> AtualizarLimite(
    string cpf,
    string conta,
    [FromBody] AtualizarLimiteRequest request)
{
    try
    {
        await _service.AtualizarLimiteAsync(cpf, conta, request);
        return Ok(new { mensagem = "Limite atualizado com sucesso." });
    }
    catch (KeyNotFoundException ex)
    {
        return NotFound(new { erro = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new { erro = ex.Message });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { erro = ex.Message });
    }
}

[HttpDelete("{cpf}/{conta}")]
public async Task<IActionResult> Remover(string cpf, string conta)
{
    try
    {
        await _service.RemoverAsync(cpf, conta);
        return Ok(new { mensagem = "Registro removido com sucesso." });
    }
    catch (KeyNotFoundException ex)
    {
        return NotFound(new { erro = ex.Message });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { erro = ex.Message });
    }
}
}