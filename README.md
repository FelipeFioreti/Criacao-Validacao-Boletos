# Cadastro e Validação de Boletos com Azure Functions

Projeto realizado durante o Bootcamp Microsoft Azure Cloud Native. Essa aplicação implementa um sistema de criação e validação de boletos utilizando **Azure Functions**. O objetivo é fornecer uma solução serverless, escalável e de fácil manutenção.

A aplicação usa Azure Functions para expor endpoints HTTP que realizam o cadastro e a validação de boletos. Após criados, os boletos são armazenados em uma fila do Azure Service Bus, permitindo o processamento assíncrono e escalável dessas mensagens.

## Funcionalidades

- Criação de boletos via API HTTP.
- Validação dos dados do boleto.
- Persistência em um Service Bus no Azure

## Tecnologias Utilizadas

- Azure Functions
- .NET / C#
- Azure Service Bus 

## Endpoints

- `POST /api/bar-code-generate`  
  Cria um novo boleto.
- `POST /api/barcode-validate`  
  Valida os dados de um boleto informado.
