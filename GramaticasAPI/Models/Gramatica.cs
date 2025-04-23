namespace GramaticasAPI.Models;

public class ProducaoDTO
{
    public string NaoTerminal { get; set; } = string.Empty;
    public List<string> Producoes { get; set; } = new();
}

public class GramaticaDTO
{
    public List<string> N { get; set; } = new(); // Não-terminais
    public List<string> T { get; set; } = new(); // Terminais
    public List<ProducaoDTO> P { get; set; } = new(); // Produções
    public string S { get; set; } = string.Empty; // Símbolo inicial
}