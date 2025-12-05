using A2.Data;
using A2.DTO;
using A2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; // Add this using
using System.Globalization;
using System.Text.Json;

namespace A2.Service
{
    public class OpenWeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OpenWeatherService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<WeatherForecastDto?> VerificarClimaAsync(decimal latitude, decimal longitude)
        {
            try
            {
                // Busca a chave e o endpoint da API diretamente da configuração
                string apiKey = _configuration["OpenWeatherMap:ApiKey"];
                string baseUrl = _configuration["OpenWeatherMap:Endpoint"];

                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(baseUrl))
                {
                    Console.WriteLine("Erro: A chave 'OpenWeatherMap:ApiKey' e/ou 'OpenWeatherMap:Endpoint' não foram encontradas na configuração da aplicação.");
                    return null;
                }

                // Garante que a URL base termine com "/weather" para a chamada de clima atual
                string correctedUrl = baseUrl.TrimEnd('/') + "/weather";

                string latStr = latitude.ToString(CultureInfo.InvariantCulture);
                string lonStr = longitude.ToString(CultureInfo.InvariantCulture);
                string url = $"{correctedUrl}?lat={latStr}&lon={lonStr}&appid={apiKey}&units=metric&lang=pt_br";

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var weatherData = JsonSerializer.Deserialize<OpenWeatherResponse>(jsonResponse);

                if (weatherData?.Weather?.Count > 0 && weatherData.Main != null)
                {
                    var condicao = weatherData.Weather[0];
                    var mainInfo = weatherData.Main;

                    var weatherDto = new WeatherForecastDto
                    {
                        Descricao = condicao.Description,
                        Temperatura = mainInfo.Temp,
                        SensacaoTermica = mainInfo.FeelsLike,
                        Icone = condicao.Icon,
                    };

                    int codigoId = condicao.Id;
                    if (codigoId >= 200 && codigoId <= 699)
                    {
                        string severidade = "Média";
                        string tipoAlerta = "Condição Adversa";

                        if (codigoId >= 200 && codigoId < 300) { severidade = "Alta"; tipoAlerta = "Tempestade"; }
                        else if (codigoId >= 500 && codigoId < 600)
                        {
                            if (codigoId == 502 || codigoId == 503 || codigoId == 504 || codigoId == 511 || codigoId >= 520) severidade = "Alta";
                            tipoAlerta = "Chuva";
                        }
                        else if (codigoId >= 600 && codigoId < 700)
                        {
                            tipoAlerta = "Neve";
                        }

                        weatherDto.TipoAlerta = tipoAlerta;
                        weatherDto.Severidade = severidade;
                    }

                    return weatherDto;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao consultar OpenWeatherMap: {ex.Message}");
            }

            return null;
        }
    }
}
