using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace valida_cpf
{
    public class FuncValidaCpf
    {
        private readonly ILogger _logger;

        public FuncValidaCpf(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FuncValidaCpf>();
        }

        [Function("funcValidaCpf")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("Iniciando a validação do CPF");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var data = JsonSerializer.Deserialize<CpfRequest>(requestBody);

            if (data == null || string.IsNullOrWhiteSpace(data.Cpf))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Por favor informe o CPF");
                return badResponse;
            }

            bool valido = ValidaCpf(data.Cpf);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync(valido ? "CPF válido" : "CPF inválido");

            return response;
        }

        public static bool ValidaCpf(string cpf)
        {
            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            if (cpf.Length != 11)
                return false;

            if (cpf.All(c => c == cpf[0]))
                return false;

            int sum = 0;
            for (int i = 0; i < 9; i++)
                sum += int.Parse(cpf[i].ToString()) * (10 - i);

            int firstDigit = sum % 11;
            firstDigit = firstDigit < 2 ? 0 : 11 - firstDigit;

            sum = 0;
            for (int i = 0; i < 9; i++)
                sum += int.Parse(cpf[i].ToString()) * (11 - i);

            sum += firstDigit * 2;

            int secondDigit = sum % 11;
            secondDigit = secondDigit < 2 ? 0 : 11 - secondDigit;

            return int.Parse(cpf[9].ToString()) == firstDigit &&
                   int.Parse(cpf[10].ToString()) == secondDigit;
        }
    }

    public class CpfRequest
    {
        public string Cpf { get; set; }
    }
}