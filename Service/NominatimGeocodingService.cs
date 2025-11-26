using A2.DTOs;
using A2.Models;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace A2.Service
{
    public class NominatimGeocodingService : IGeocodingService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://nominatim.openstreetmap.org/search";

        // Injetamos o HttpClient (que será configurado no Program.cs)
        public NominatimGeocodingService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // REGRA DO NOMINATIM: É OBRIGATÓRIO identificar sua aplicação.
            // Use um nome único para seu projeto ou seu e-mail.
            _httpClient.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("LogiFleetApp_StudentProject", "1.0"));
        }

        public async Task<(decimal Latitude, decimal Longitude)?> ObterCoordenadasAsync(EnderecoCliente endereco)
        {
            try
            {
                // 1. Montar a query string de busca
                // Ex: Rua X, 123, Cidade Y, UF
                string query = $"{endereco.Logradouro}, {endereco.Numero}, {endereco.Cidade}, {endereco.UF}, Brazil";
                string encodedQuery = Uri.EscapeDataString(query);

                // Url final: https://nominatim.openstreetmap.org/search?q=Rua...&format=json&limit=1
                string url = $"{BaseUrl}?q={encodedQuery}&format=json&limit=1";

                // 2. Fazer a chamada HTTP GET
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Lança erro se não for 200 OK

                // 3. Ler o conteúdo
                string jsonResponse = await response.Content.ReadAsStringAsync();

                // O Nominatim retorna uma LISTA de resultados (colchetes [])
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var results = JsonSerializer.Deserialize<List<NominatimResult>>(jsonResponse, options);

                if (results != null && results.Count > 0)
                {
                    var primeiroResultado = results[0];

                    // Converter as strings "lat" e "lon" para decimal usando cultura invariante (ponto como separador)
                    if (decimal.TryParse(primeiroResultado.Lat, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal lat) &&
                        decimal.TryParse(primeiroResultado.Lon, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal lon))
                    {
                        // Sucesso! Retorna a tupla com as coordenadas
                        return (lat, lon);
                    }
                }
            }
            catch (Exception ex)
            {
                // Em um sistema real, você logaria este erro (ex: usando ILogger)
                Console.WriteLine($"Erro na geocodificação: {ex.Message}");
                // Se der erro, retornamos null e o endereço será salvo com 0.0, 0.0
            }

            // Se não achou nada ou deu erro
            return null;
        }
    }
}
