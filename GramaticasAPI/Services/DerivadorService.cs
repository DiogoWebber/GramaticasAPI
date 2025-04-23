using System;
using System.Collections.Generic;
using System.Linq;
using GramaticasAPI.Interfaces;
using GramaticasAPI.Models;
using GramaticasAPI.Response;

namespace GramaticasAPI.Services
{
    public class DerivadorService : IDerivador
    {
        public GenericResponse<DerivacaoResultado> Derivar(GramaticaDTO gramatica)
        {
            try
            {
                // Validações iniciais da gramática
                if (gramatica == null)
                {
                    return GenericResponse<DerivacaoResultado>.ErroResponse(
                        new List<string> { "Gramática não pode ser nula." },
                        "Erro de validação"
                    );
                }
                if (string.IsNullOrEmpty(gramatica.S))
                {
                    return GenericResponse<DerivacaoResultado>.ErroResponse(
                        new List<string> { "Símbolo inicial é obrigatório." },
                        "Erro de validação"
                    );
                }
                if (gramatica.N == null || gramatica.N.Count == 0)
                {
                    return GenericResponse<DerivacaoResultado>.ErroResponse(
                        new List<string> { "Conjunto de não-terminais está vazio." },
                        "Erro de validação"
                    );
                }
                if (gramatica.T == null || gramatica.T.Count == 0)
                {
                    return GenericResponse<DerivacaoResultado>.ErroResponse(
                        new List<string> { "Conjunto de terminais está vazio." },
                        "Erro de validação"
                    );
                }
                if (gramatica.P == null || gramatica.P.Count == 0)
                {
                    return GenericResponse<DerivacaoResultado>.ErroResponse(
                        new List<string> { "Conjunto de produções está vazio." },
                        "Erro de validação"
                    );
                }
                // Verifica se o símbolo inicial pertence ao conjunto de não-terminais
                if (!gramatica.N.Contains(gramatica.S))
                {
                    return GenericResponse<DerivacaoResultado>.ErroResponse(
                        new List<string> { "O símbolo inicial deve pertencer ao conjunto de não-terminais." },
                        "Erro de validação"
                    );
                }

                // Validações das produções – garantindo que elas sejam compatíveis com gramáticas regulares:
                // Para produções com comprimento 1, o símbolo deve ser terminal.
                // Para produções com comprimento > 1, o último símbolo deve ser um não-terminal e os demais, terminais.
                foreach (var regra in gramatica.P)
                {
                    if (!gramatica.N.Contains(regra.NaoTerminal))
                    {
                        return GenericResponse<DerivacaoResultado>.ErroResponse(
                            new List<string> { $"O não-terminal '{regra.NaoTerminal}' não pertence ao conjunto de não-terminais." },
                            "Erro de validação"
                        );
                    }
                    if (regra.Producoes == null || regra.Producoes.Count == 0)
                    {
                        return GenericResponse<DerivacaoResultado>.ErroResponse(
                            new List<string> { $"A regra para o não-terminal '{regra.NaoTerminal}' não possui produções." },
                            "Erro de validação"
                        );
                    }
                    foreach (var producao in regra.Producoes)
                    {
                        if (string.IsNullOrWhiteSpace(producao))
                        {
                            return GenericResponse<DerivacaoResultado>.ErroResponse(
                                new List<string> { $"Produção vazia encontrada para o não-terminal '{regra.NaoTerminal}'." },
                                "Erro de validação"
                            );
                        }
                        // Se a produção tem somente um caractere, deve ser terminal
                        if (producao.Length == 1)
                        {
                            if (!gramatica.T.Contains(producao))
                            {
                                return GenericResponse<DerivacaoResultado>.ErroResponse(
                                    new List<string> { $"Na produção de '{regra.NaoTerminal}', o símbolo '{producao}' deve ser terminal." },
                                    "Erro de validação"
                                );
                            }
                        }
                        else
                        {
                            // Se a produção tem mais de um caractere, o último deve ser um não-terminal e o restante, terminais.
                            char ultimo = producao[producao.Length - 1];
                            if (!gramatica.N.Contains(ultimo.ToString()))
                            {
                                return GenericResponse<DerivacaoResultado>.ErroResponse(
                                    new List<string> { $"Produção '{producao}' em '{regra.NaoTerminal}' é irregular. " +
                                    "Quando a produção tiver mais de um caractere, o último deve ser um não-terminal." },
                                    "Erro de validação"
                                );
                            }
                            string parteTerminais = producao.Substring(0, producao.Length - 1);
                            foreach (var simbolo in parteTerminais)
                            {
                                if (!gramatica.T.Contains(simbolo.ToString()))
                                {
                                    return GenericResponse<DerivacaoResultado>.ErroResponse(
                                        new List<string> { $"Símbolo '{simbolo}' na produção '{producao}' deve ser terminal." },
                                        "Erro de validação"
                                    );
                                }
                            }
                        }
                    }
                }

                // Derivação utilizando pilha:
                var pilha = new Stack<char>();
                var resultado = "";
                var passos = new List<string>();
                var rand = new Random();
                
                // Empilha o símbolo inicial (assumindo que ele é representado por um único caractere)
                char simboloInicial = gramatica.S[0];
                pilha.Push(simboloInicial);

                // Registra a derivação inicial (a forma sentencial é o resultado já gerado concatenado com o não-terminal pendente, se houver)
                passos.Add(resultado + (pilha.Count > 0 ? pilha.Peek().ToString() : ""));

                // Define um limite de iterações para evitar loops infinitos
                int limiteIteracoes = 100;
                int iteracao = 0;

                while (pilha.Count > 0)
                {
                    if (++iteracao > limiteIteracoes)
                    {
                        return GenericResponse<DerivacaoResultado>.ErroResponse(
                            new List<string> { "Limite de iterações atingido. Possível loop infinito." },
                            "Erro na derivação"
                        );
                    }

                    // Retira o símbolo do topo da pilha para expandir
                    char simboloAtual = pilha.Pop();

                    // Procura a regra de produção para o não-terminal atual
                    var regraAplicavel = gramatica.P.FirstOrDefault(r => r.NaoTerminal == simboloAtual.ToString());
                    if (regraAplicavel == null)
                    {
                        return GenericResponse<DerivacaoResultado>.ErroResponse(
                            new List<string> { $"Não existe uma regra para o não-terminal '{simboloAtual}'." },
                            "Erro na derivação"
                        );
                    }

                    // Escolhe uma produção aleatória para este não-terminal
                    string producaoEscolhida = regraAplicavel.Producoes[rand.Next(regraAplicavel.Producoes.Count)];

                    // Se a produção tiver comprimento 1, ela deve ser composta apenas por um terminal:
                    if (producaoEscolhida.Length == 1)
                    {
                        resultado += producaoEscolhida;
                    }
                    else
                    {
                        // Para produção do tipo "wX":
                        // 'w' é a parte terminal e 'X' é o não-terminal que será expandido futuramente.
                        string parteTerminal = producaoEscolhida.Substring(0, producaoEscolhida.Length - 1);
                        char novoNaoTerminal = producaoEscolhida[producaoEscolhida.Length - 1];

                        resultado += parteTerminal;
                        pilha.Push(novoNaoTerminal);
                    }

                    // A forma sentencial atual é composta pelo que já foi gerado (resultado)
                    // concatenado com o não-terminal pendente (se houver) – lembrando que,
                    // para gramáticas regulares, há no máximo um não-terminal.
                    string estadoAtual = resultado + (pilha.Count > 0 ? pilha.Peek().ToString() : "");
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
    }
}
