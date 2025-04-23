using GramaticasAPI.Models;
using GramaticasAPI.Response;

namespace GramaticasAPI.Interfaces;

public interface IDerivador
{
     GenericResponse<DerivacaoResultado> Derivar(GramaticaDTO gramatica);
}