namespace GramaticasAPI.Models;

public class DerivacaoResultado
{
    public string SentencaFinal { get; set; } = string.Empty;
    public List<string> Passos { get; set; } = new();
}