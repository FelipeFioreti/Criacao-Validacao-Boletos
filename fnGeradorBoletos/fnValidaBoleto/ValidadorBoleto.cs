using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace fnValidaBoleto;

public class ValidadorBoleto
{
    private readonly ILogger<ValidadorBoleto> _logger;

    public ValidadorBoleto(ILogger<ValidadorBoleto> logger)
    {
        _logger = logger;
    }

    [Function("barcode-validate")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        dynamic data = JsonConvert.DeserializeObject(requestBody);

        string barcodeData = data?.barcode;

        if (string.IsNullOrEmpty(barcodeData))
        {
            return new BadRequestObjectResult("O campo barcode deve ser obrigatório.");
        }

        if( barcodeData.Length != 44)
        {
            var result = new
            {

                valido = false,
                mensagem = "O campo barcode deve conter 44 caracteres."
            };
            return new BadRequestObjectResult(result);
        }

        string datePart = barcodeData.Substring(3, 8);
        if (!DateTime.TryParseExact(datePart, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime dateObj))
        {
            var result = new
            {
                valido = false,
                mensagem = "Data de Vencimento Inválida."
            };
            return new BadRequestObjectResult(result);
        }

        var resultSuccess = new
        {
            valido = true,
            mensagem = "Boleto válido.",
            vencimento = dateObj.ToString("dd/MM/yyyy")
        };

        return new OkObjectResult(resultSuccess);
    }
}