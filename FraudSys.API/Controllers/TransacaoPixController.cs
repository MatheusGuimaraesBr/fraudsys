using FraudSys.Application.DTOs;
using FraudSys.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FraudSys.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransacaoPixController : ControllerBase
{
    private readonly LimiteContaService _service;

    public TransacaoPixController(LimiteContaService service)
    {
        _service = service;
    }

[HttpPost]
public async Task<IActionResult> ProcessarTransacao([FromBody] TransacaoPixRequest request)
{
    try
    {
        var resultado = await _service.ProcessarTransacaoAsync(request);

        if (!resultado.Aprovada)
            return UnprocessableEntity(resultado);

        return Ok(resultado);
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
}