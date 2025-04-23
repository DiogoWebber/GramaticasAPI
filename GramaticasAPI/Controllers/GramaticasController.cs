using GramaticasAPI.Interfaces;
using GramaticasAPI.Models;
using GramaticasAPI.Response;
using Microsoft.AspNetCore.Mvc;

namespace GramaticasAPI.Controllers
{
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

            if (!resultado.Sucesso)
            {
                return BadRequest(resultado);
            }

            return Ok(resultado);
        }
    }
}