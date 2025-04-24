using GramaticasAPI.Interfaces;
using GramaticasAPI.Models;
using GramaticasAPI.Response;

namespace GramaticasAPI.Services;

public class DerivadorService : IDerivador
{
    public GenericResponse<DerivacaoResultado> Derivar(GramaticaDTO gramatica)
    {
        try
        {
            // Validações iniciais da gramática
            if (gramatica == null)
                return GenericResponse<DerivacaoResultado>.ErroResponse(
                    new List<string> { "Gramática não pode ser nula." },
                    "Erro de validação"
                );
            if (string.IsNullOrEmpty(gramatica.S))
                return GenericResponse<DerivacaoResultado>.ErroResponse(
                    new List<string> { "Símbolo inicial é obrigatório." },
                    "Erro de validação"
                );
            if (gramatica.N == null || gramatica.N.Count == 0)
                return GenericResponse<DerivacaoResultado>.ErroResponse(
                    new List<string> { "Conjunto de não-terminais está vazio." },
                    "Erro de validação"
                );
            if (gramatica.T == null || gramatica.T.Count == 0)
                return GenericResponse<DerivacaoResultado>.ErroResponse(
                    new List<string> { "Conjunto de terminais está vazio." },
                    "Erro de validação"
                );
            if (gramatica.P == null || gramatica.P.Count == 0)
                return GenericResponse<DerivacaoResultado>.ErroResponse(
                    new List<string> { "Conjunto de produções está vazio." },
                    "Erro de validação"
                );
            // Verifica se o símbolo inicial pertence ao conjunto de não-terminais
            if (!gramatica.N.Contains(gramatica.S))
                return GenericResponse<DerivacaoResultado>.ErroResponse(
                    new List<string> { "O símbolo inicial deve pertencer ao conjunto de não-terminais." },
                    "Erro de validação"
                );

            // Validações das produções – garantindo que elas sejam compatíveis com gramáticas regulares:
            foreach (var regra in gramatica.P)
            {
                if (!gramatica.N.Contains(regra.NaoTerminal))
                    return GenericResponse<DerivacaoResultado>.ErroResponse(
                        new List<string>
                            { $"O não-terminal '{regra.NaoTerminal}' não pertence ao conjunto de não-terminais." },
                        "Erro de validação"
                    );
                if (regra.Producoes == null || regra.Producoes.Count == 0)
                    return GenericResponse<DerivacaoResultado>.ErroResponse(
                        new List<string> { $"A regra para o não-terminal '{regra.NaoTerminal}' não possui produções." },
                        "Erro de validação"
                    );
                foreach (var producao in regra.Producoes)
                {
                    if (string.IsNullOrWhiteSpace(producao))
                        return GenericResponse<DerivacaoResultado>.ErroResponse(
                            new List<string>
                                { $"Produção vazia encontrada para o não-terminal '{regra.NaoTerminal}'." },
                            "Erro de validação"
                        );
                    if (producao == "ε")
                    {
                        continue;
                    }
                    if (producao.Length == 1)
                    {
                        if (!gramatica.T.Contains(producao))
                            return GenericResponse<DerivacaoResultado>.ErroResponse(
                                new List<string>
                                {
                                    $"Na produção de '{regra.NaoTerminal}', o símbolo '{producao}' deve ser terminal."
                                },
                                "Erro de validação"
                            );
                    }
                    else
                    {
                        var ultimo = producao[producao.Length - 1];
                        if (!gramatica.N.Contains(ultimo.ToString()))
                            return GenericResponse<DerivacaoResultado>.ErroResponse(
                                new List<string>
                                {
                                    $"Produção '{producao}' em '{regra.NaoTerminal}' é irregular. " +
                                    "Quando a produção tiver mais de um caractere, o último deve ser um não-terminal."
                                },
                                "Erro de validação"
                            );
                        var parteTerminais = producao.Substring(0, producao.Length - 1);
                        foreach (var simbolo in parteTerminais)
                            if (!gramatica.T.Contains(simbolo.ToString()))
                                return GenericResponse<DerivacaoResultado>.ErroResponse(
                                    new List<string>
                                        { $"Símbolo '{simbolo}' na produção '{producao}' deve ser terminal." },
                                    "Erro de validação"
                                );
                    }
                }
            }

            // Derivação utilizando pilha:
            var pilha = new Stack<char>();
            var resultado = "";
            var passos = new List<string>();
            var rand = new Random();

            // Empilha o símbolo inicial
            var simboloInicial = gramatica.S[0];
            pilha.Push(simboloInicial);

            // Registra a derivação inicial
            passos.Add(resultado + (pilha.Count > 0 ? pilha.Peek().ToString() : ""));

            // Define um limite de iterações para evitar loops infinitos
            var limiteIteracoes = 100;
            var iteracao = 0;

            while (pilha.Count > 0)
            {
                if (++iteracao > limiteIteracoes)
                    return GenericResponse<DerivacaoResultado>.ErroResponse(
                        new List<string> { "Limite de iterações atingido. Possível loop infinito." },
                        "Erro na derivação"
                    );

                var simboloAtual = pilha.Pop();

                var regraAplicavel = gramatica.P.FirstOrDefault(r => r.NaoTerminal == simboloAtual.ToString());
                if (regraAplicavel == null)
                    return GenericResponse<DerivacaoResultado>.ErroResponse(
                        new List<string> { $"Não existe uma regra para o não-terminal '{simboloAtual}'." },
                        "Erro na derivação"
                    );

                var producaoEscolhida = regraAplicavel.Producoes[rand.Next(regraAplicavel.Producoes.Count)];

                if (producaoEscolhida == "ε")
                {
                    // Registra no histórico de passos
                    passos.Add(resultado + "ε");
                }
                else
                {
                    if (producaoEscolhida.Length == 1)
                    {
                        resultado += producaoEscolhida; // Adiciona o terminal ao resultado
                    }
                    else
                    {
                        var parteTerminal = producaoEscolhida.Substring(0, producaoEscolhida.Length - 1);
                        var novoNaoTerminal = producaoEscolhida[producaoEscolhida.Length - 1];

                        resultado += parteTerminal; // Adiciona os terminais ao resultado
                        pilha.Push(novoNaoTerminal); // Empilha o não-terminal para expansão futura
                    }
                    
                }

                // Adiciona o estado atual à lista de passos
                var estadoAtual = resultado + (pilha.Count > 0 ? pilha.Peek().ToString() : "");
                passos.Add(estadoAtual);
            }

            return GenericResponse<DerivacaoResultado>.SucessoResponse(
                new DerivacaoResultado { SentencaFinal = resultado, Passos = passos },
                "Sentença derivada com sucesso"
            );
        }
        catch (Exception e)
        {
            return GenericResponse<DerivacaoResultado>.ErroResponse(
                new List<string> { e.Message },
                "Erro inesperado"
            );
        }
    }

    public static class GramaticaParser
    {
        public static GramaticaDTO Parse(string input)
        {
            var linhas = input.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var naoTerminais = new HashSet<string>();
            var terminais = new HashSet<string>();
            var producoes = new List<ProducaoDTO>();
            string simboloInicial = null;

            foreach (var linha in linhas)
            {
                var partes = linha.Split('=', 2);
                if (partes.Length != 2) throw new Exception($"Linha inválida: {linha}");

                var naoTerminal = partes[0].Trim();
                if (string.IsNullOrEmpty(simboloInicial))
                    simboloInicial = naoTerminal;

                naoTerminais.Add(naoTerminal);

                var producoesStr = partes[1].Split('|', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .ToList();

                foreach (var p in producoesStr)
                foreach (var simbolo in p)
                {
                    var s = simbolo.ToString();
                    if (s == "ε") continue;
                    if (char.IsUpper(simbolo))
                        naoTerminais.Add(s);
                    else
                        terminais.Add(s);
                }

                producoes.Add(new ProducaoDTO
                {
                    NaoTerminal = naoTerminal,
                    Producoes = producoesStr
                });
            }

            return new GramaticaDTO
            {
                S = simboloInicial,
                N = naoTerminais.ToList(),
                T = terminais.ToList(),
                P = producoes
            };
        }
    }
}
