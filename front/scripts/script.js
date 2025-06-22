
const form = document.getElementById('boleto-form');
const resultDiv = document.getElementById('result');
const errorDiv = document.getElementById('error');

form.addEventListener('submit', async (e) => {
    e.preventDefault();
    resultDiv.innerHTML = '';
    errorDiv.textContent = '';

    const valor = document.getElementById('valor').value;
    const data = document.getElementById('data').value;

    if (!valor || !data) {
    errorDiv.textContent = 'Preencha todos os campos.';
    return;
    }

    try {
    // Troque a URL abaixo pela URL real da sua API
    const response = await fetch('http://localhost:7088/api/bar-code-generate', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
        valor: valor,
        dataVencimento: data
        })
    });

    if (!response.ok) {
        const err = await response.text();
        throw new Error(err);
    }

    const dataResp = await response.json();

    resultDiv.innerHTML = `
        <div class="info"><strong>Código de Barras:</strong><br>${dataResp.barCode}</div>
        <div class="info"><strong>Valor:</strong> R$ ${Number(dataResp.valorOriginal).toFixed(2)}</div>
        <div class="info"><strong>Vencimento:</strong> ${new Date(dataResp.dataVencimento).toLocaleDateString()}</div>
        <img class="barcode-img" src="data:image/png;base64,${dataResp.imagemBase64}" alt="Código de Barras">
    `;
    } catch (err) {
    errorDiv.textContent = 'Erro ao gerar boleto: ' + err.message;
    }
});

const validarForm = document.getElementById('validar-form');
const validarResult = document.getElementById('validar-result');
const validarError = document.getElementById('validar-error');

if (validarForm) {
  validarForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    validarResult.textContent = '';
    validarError.textContent = '';

    const codigo = document.getElementById('codigo-barras').value.trim();

    if (!codigo) {
      validarError.textContent = 'Insira o código de barras.';
      return;
    }

    try {
      // Troque a URL abaixo pela URL real da sua API de validação
      const response = await fetch('http://localhost:7169/api/barcode-validate', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ barcode: codigo })
      });

      if (!response.ok) {
        const err = await response.json();
        validarResult.textContent = '❌ Boleto inválido!';
        throw new Error(err.mensagem);
      }

      const data = await response.json();

      if (data.valido) {
        validarResult.textContent = '✅ Boleto válido!';
      } 
    } catch (err) {
      validarError.textContent = err.message;
    }
  });
}   
