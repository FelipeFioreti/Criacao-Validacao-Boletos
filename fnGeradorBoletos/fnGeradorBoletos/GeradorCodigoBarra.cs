using Azure.Messaging.ServiceBus;
using BarcodeStandard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace fnGeradorBoletos;

public class GeradorCodigoBarra
{
    private readonly ILogger<GeradorCodigoBarra> _logger;
    private readonly string _connectionString;
    private readonly string _queueName = "gerador-codigo-barras";

    public GeradorCodigoBarra(ILogger<GeradorCodigoBarra> logger)
    {
        _logger = logger;
        _connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
    }

    [Function("bar-code-generate")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string valor = data?.valor;
            string dataVencimento = data?.dataVencimento;

            string barCodeData;

            // Validar dados
            if (string.IsNullOrEmpty(valor) || string.IsNullOrEmpty(dataVencimento))
            {
                return new BadRequestObjectResult("Valor e data de vencimento são obrigatórios.");
            }

            // Validar data
            if(!DateTime.TryParseExact(dataVencimento,"yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime dateObj))
            {
                return new BadRequestObjectResult("Data de vencimento inválida.");
            }

            string dateStr = dateObj.ToString("yyyyMMdd");

            // Conversão do valor para centavos
            if (!decimal.TryParse(valor, out decimal valorDecimal))
            {
                return new BadRequestObjectResult("Valor inválido. Deve ser um número decimal.");
            }

            int valorCentavos = (int)(valorDecimal * 100);
            string valorStr = valorCentavos.ToString("D8");

            string bankCode = "008";
            string baseCode = string.Concat(bankCode, dateStr, valorStr);

            //  Criando o código de barras com 44 caracteres

            barCodeData = baseCode.Length < 44 ? baseCode.PadRight(44, '0') : baseCode.Substring(0, 44);
            _logger.LogInformation($"Código de barras gerado: {barCodeData}");

            Barcode barcode = new Barcode();

            var skImage = barcode.Encode(BarcodeStandard.Type.Code128, barCodeData);
            
            using (var encodeData = skImage.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100))
            {
                var imageBytes = encodeData.ToArray();

                string base64Image = Convert.ToBase64String(imageBytes);

                var resultObject = new
                {
                    barCode = barCodeData,
                    valorOriginal = valorDecimal,
                    dataVencimento = DateTime.Now.AddDays(5),
                    imagemBase64 = base64Image
                };

                await SendFileFallback(resultObject, _connectionString, _queueName);
                return new OkObjectResult(resultObject);
            }


        }
        catch (Exception ex)
        {
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
        
    }

    private async Task SendFileFallback(object resultObject, string connectionString, string queueName)
    {
        await using var client = new ServiceBusClient(connectionString);

        ServiceBusSender sender = client.CreateSender(queueName);

        var messageBody = JsonConvert.SerializeObject(resultObject);

        ServiceBusMessage message = new ServiceBusMessage(messageBody);

        await sender.SendMessageAsync(message); 

        _logger.LogInformation($"Mensagem enviada para a fila {queueName}");
    }
}