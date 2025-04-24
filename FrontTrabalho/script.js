function derivar() {
    const gramaticaTexto = document.getElementById("gramatica").value;
  
    fetch('http://localhost:5002/api/Gramaticas/derivar-simples', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(gramaticaTexto)
    })
      .then(async response => {
        const data = await response.json();
  
        if (!response.ok) {
          // Exibe os erros do backend se houverem
          if (data.erros && data.erros.length > 0) {
            alert("Erro(s) ao derivar:\n" + data.erros.join('\n'));
          } else {
            alert("Erro ao derivar: " + data.mensagem || "Erro desconhecido.");
          }
          throw new Error("Erro da API.");
        }
  
        if (!data.sucesso || !data.dados) {
          alert("Erro ao derivar: " + data.mensagem);
          return;
        }
  
        const sentencaFinal = data.dados.sentencaFinal;
        const passos = data.dados.passos;
  
        document.getElementById("sentencaFinal").innerText = sentencaFinal;
        const passosLista = document.getElementById("passosLista");
        passosLista.innerHTML = "";
        passos.forEach(passo => {
          const li = document.createElement("li");
          li.textContent = passo;
          passosLista.appendChild(li);
        });
  
        document.getElementById("resultado").style.display = "block";
      })
      .catch(error => {
        console.error("Erro ao chamar API:", error);
      });
  }
  