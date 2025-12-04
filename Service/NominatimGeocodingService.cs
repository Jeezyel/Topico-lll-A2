using A2.Data;
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
        private readonly A2Context _context;
        private const string BaseUrl = "https://nominatim.openstreetmap.org/search";

        public NominatimGeocodingService(HttpClient httpClient, A2Context context)
        {
            _httpClient = httpClient;
            _context = context;

            _httpClient.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("LogiFleetApp_StudentProject", "1.0"));
        }

        public async Task<(decimal Latitude, decimal Longitude)?> ObterCoordenadasAsync(EnderecoCliente endereco)
        {
            try
            {
                string query = $"{endereco.Logradouro}, {endereco.Numero}, {endereco.Bairro} ,{endereco.Cidade}, {endereco.UF}, {endereco.CEP}, Brazil";
                string encodedQuery = Uri.EscapeDataString(query);
                string url = $"{BaseUrl}?q={encodedQuery}&format=json&limit=1";
                
                // Log the integration attempt
                var log = new LogIntegracao { ApiNome = "Nominatim", Endpoint = BaseUrl, DataHora = DateTime.UtcNow };
                _context.LogsIntegracao.Add(log);
                await _context.SaveChangesAsync();

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var results = JsonSerializer.Deserialize<List<NominatimResult>>(jsonResponse, options);

                if (results?.Count > 0)
                {
                    var primeiroResultado = results[0];
                    if (decimal.TryParse(primeiroResultado.Lat, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal lat) &&
                        decimal.TryParse(primeiroResultado.Lon, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal lon))
                    {
                        return (lat, lon);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na geocodificação: {ex.Message}");
            }

            return null;
        }
    }
}
