using A2.Data;
using A2.DTO;
using A2.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace A2.Service
{
    public class OpenWeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly A2Context _context;

        public OpenWeatherService(HttpClient httpClient, A2Context context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        public async Task<AlertaClimatico?> VerificarClimaAsync(decimal latitude, decimal longitude)
        {
            try
            {
                // 1. Obter configuração do banco de dados
                var config = await _context.ConfiguracoesSistema
                                           .FirstOrDefaultAsync(c => c.ApiNome == "OpenWeather");
                if (config == null)
                {
                    Console.WriteLine("Erro: Configuração para 'OpenWeather' não encontrada no banco de dados.");
                    return null;
                }
                
                string apiKey = config.Valor;
                string baseUrl = config.Endpoint;

                string latStr = latitude.ToString(CultureInfo.InvariantCulture);
                string lonStr = longitude.ToString(CultureInfo.InvariantCulture);
                string url = $"{baseUrl}?lat={latStr}&lon={lonStr}&appid={apiKey}&units=metric&lang=pt_br";

                // 2. Logar a tentativa de integração
                var log = new LogIntegracao { ApiNome = "OpenWeather", Endpoint = baseUrl, DataHora = DateTime.UtcNow };
                _context.LogsIntegracao.Add(log);
                await _context.SaveChangesAsync();


                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var weatherData = JsonSerializer.Deserialize<OpenWeatherResponse>(jsonResponse);

                if (weatherData?.Weather?.Count > 0)
                {
                    var condicao = weatherData.Weather[0];
                    int codigoId = condicao.Id;

                    if (codigoId >= 200 && codigoId <= 699)
                    {
                        string severidade = "Média";
                        string tipoAlerta = "Chuva/Neve";

                        if (codigoId >= 200 && codigoId < 300) { severidade = "Alta"; tipoAlerta = "Tempestade"; }
                        else if (codigoId >= 500 && codigoId < 600)
                        {
                            if (codigoId == 502 || codigoId == 503 || codigoId == 504 || codigoId == 511 || codigoId >= 520) severidade = "Alta";
                            tipoAlerta = "Chuva";
                        }
                        
                        return new AlertaClimatico
                        {
                            TipoAlerta = tipoAlerta + ": " + condicao.Description,
                            Severidade = severidade
                        };
                    }
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
