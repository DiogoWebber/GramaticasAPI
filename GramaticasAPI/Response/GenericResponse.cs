namespace GramaticasAPI.Response;

public class GenericResponse<T>
{
    public bool Sucesso { get; set; }
    public string? Mensagem { get; set; }
    public T? Dados { get; set; }
    public List<string>? Erros { get; set; }

    public static GenericResponse<T> SucessoResponse(T dados, string mensagem = "")
    {
        return new GenericResponse<T>
        {
            Sucesso = true,
            Mensagem = mensagem,
            Dados = dados,
            Erros = null
        };
    }

    public static GenericResponse<T> ErroResponse(List<string> erros, string mensagem = "")
    {
        return new GenericResponse<T>
        {
            Sucesso = false,
            Mensagem = mensagem,
            Dados = default,
            Erros = erros
        };
    }
}