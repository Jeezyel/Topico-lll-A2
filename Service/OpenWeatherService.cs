using A2.DTO;
using A2.Models;
using System.Globalization;
using System.Text.Json;

namespace A2.Service
{
    public class OpenWeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string BaseUrl = "https://api.openweathermap.org/data/2.5/weather";

        // Injetamos o HttpClient e a Configuração para ler a chave
        public OpenWeatherService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenWeatherMap:ApiKey"]; // Lê a chave do appsettings.json
        }

        public async Task<AlertaClimatico?> VerificarClimaAsync(decimal latitude, decimal longitude)
        {
            try
            {
                // Usa CultureInfo.InvariantCulture para garantir que o decimal use ponto (.) e não vírgula (,) na URL
                string latStr = latitude.ToString(CultureInfo.InvariantCulture);
                string lonStr = longitude.ToString(CultureInfo.InvariantCulture);

                // Monta a URL: Base + Lat + Lon + Chave + Unidades(métrico) + Idioma(pt_br)
                string url = $"{BaseUrl}?lat={latStr}&lon={lonStr}&appid={_apiKey}&units=metric&lang=pt_br";

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var weatherData = JsonSerializer.Deserialize<OpenWeatherResponse>(jsonResponse);

                // Verifica se veio alguma informação de clima
                if (weatherData != null && weatherData.Weather != null && weatherData.Weather.Count > 0)
                {
                    var condicao = weatherData.Weather[0];
                    int codigoId = condicao.Id;

                    // --- REGRA DE NEGÓCIO PARA GERAR ALERTA ---
                    // Se o código começar com 2 (Tempestade), 5 (Chuva) ou 6 (Neve)
                    // Documentação dos códigos: https://openweathermap.org/weather-conditions
                    if (codigoId >= 200 && codigoId <= 699)
                    {
                        // Definir a severidade baseada no grupo
                        string severidade = "Média";
                        string tipoAlerta = "Chuva/Neve";

                        if (codigoId >= 200 && codigoId < 300)
                        {
                            severidade = "Alta";
                            tipoAlerta = "Tempestade";
                        }
                        else if (codigoId >= 500 && codigoId < 600) // Chuvas mais intensas
                        {
                            if (codigoId == 502 || codigoId == 503 || codigoId == 504 || codigoId == 511 || codigoId >= 520)
                            {
                                severidade = "Alta";
                            }
                            tipoAlerta = "Chuva";
                        }

                        // Retorna o objeto de alerta pronto (sem salvar ainda)
                        return new AlertaClimatico
                        {
                            TipoAlerta = tipoAlerta + ": " + condicao.Description,
                            Severidade = severidade
                            // RotaId será preenchido por quem chamou o serviço
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                // Em produção, logar o erro.
                Console.WriteLine($"Erro ao consultar OpenWeatherMap: {ex.Message}");
            }

            // Se o tempo estiver bom (códigos 800, 80x) ou der erro, não gera alerta.
            return null;
        }
    }
}
