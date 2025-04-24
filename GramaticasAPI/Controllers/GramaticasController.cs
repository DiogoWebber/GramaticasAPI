using GramaticasAPI.Interfaces;
using GramaticasAPI.Models;
using GramaticasAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GramaticasAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GramaticasController : ControllerBase
{
    private readonly IDerivador _derivador;

    public GramaticasController(IDerivador derivador)
    {
        _derivador = derivador;
    }

    [HttpPost("derivar")]
    public IActionResult DerivarSentenca([FromBody] GramaticaDTO gramatica)
    {
        var resultado = _derivador.Derivar(gramatica);

        if (!resultado.Sucesso) return BadRequest(resultado);

        return Ok(resultado);
    }

    [HttpPost("derivar-simples")]
    public IActionResult DerivarSimples([FromBody] string entradaSimples)
    {
        try
        {
            var gramatica = DerivadorService.GramaticaParser.Parse(entradaSimples);
            return DerivarSentenca(gramatica);
        }
        catch (Exception ex)
        {
            return BadRequest(new { erro = "Erro ao processar entrada simples", detalhes = ex.Message });
        }
    }
}