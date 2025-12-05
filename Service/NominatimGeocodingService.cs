using A2.Data;
using A2.DTOs;
using A2.Models;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace A2.Service
{
    public class NominatimGeocodingService : IGeocodingService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://nominatim.openstreetmap.org/search";

        public NominatimGeocodingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Nominatim requires a valid User-Agent header.
            _httpClient.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("LogiFleetApp_StudentProject", "1.0"));
        }

        private async Task<List<NominatimResult>> ExecuteQueryAsync(string query)
        {
            try
            {
                string encodedQuery = Uri.EscapeDataString(query);
                string url = $"{BaseUrl}?q={encodedQuery}&format=json&limit=1";
                
                Console.WriteLine($"[GEO-LOG] Tentando geocodificar com a consulta: \"{query}\"");

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<NominatimResult>>(jsonResponse, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GEO-LOG] ERRO ao executar a consulta \"{query}\": {ex.Message}");
                return null;
            }
        }

        public async Task<(decimal Latitude, decimal Longitude)?> ObterCoordenadasAsync(EnderecoCliente endereco)
        {
            if (string.IsNullOrWhiteSpace(endereco.Logradouro) || string.IsNullOrWhiteSpace(endereco.Cidade) || string.IsNullOrWhiteSpace(endereco.UF))
            {
                Console.WriteLine("[GEO-LOG] AVISO: Geocodificação pulada pois campos essenciais (Logradouro, Cidade, UF) estão vazios.");
                return null;
            }

            // Lista de consultas para tentar, em ordem de especificidade
            var queries = new List<string>();
            var queryParts = new List<string>();

            // --- Lógica para construir as queries de forma mais robusta ---

            // 1. Consulta mais específica (com todos os dados)
            queryParts.Clear();
            queryParts.Add(endereco.Logradouro);
            if (!string.IsNullOrWhiteSpace(endereco.Numero) && endereco.Numero != "0" && endereco.Numero != "00")
            {
                queryParts.Add(endereco.Numero);
            }
            if (!string.IsNullOrWhiteSpace(endereco.Bairro))
            {
                queryParts.Add(endereco.Bairro);
            }
            queryParts.Add(endereco.Cidade);
            queryParts.Add(endereco.UF);
            queryParts.Add("Brazil");
            queries.Add(string.Join(", ", queryParts));

            // 2. Consulta um pouco menos específica (sem bairro)
            queryParts.Clear();
            queryParts.Add(endereco.Logradouro);
            if (!string.IsNullOrWhiteSpace(endereco.Numero) && endereco.Numero != "0" && endereco.Numero != "00")
            {
                queryParts.Add(endereco.Numero);
            }
            queryParts.Add(endereco.Cidade);
            queryParts.Add(endereco.UF);
            queryParts.Add("Brazil");
            queries.Add(string.Join(", ", queryParts));
    
            // 3. Consulta ainda menos específica (sem número e bairro)
            queries.Add($"{endereco.Logradouro}, {endereco.Cidade}, {endereco.UF}, Brazil");

            // 4. Consulta pelo CEP (se disponível)
            if (!string.IsNullOrWhiteSpace(endereco.CEP))
            {
                queries.Add($"{endereco.CEP}, Brazil");
            }

            foreach (var query in queries.Distinct()) // Usa Distinct para não repetir consultas idênticas
            {
                var results = await ExecuteQueryAsync(query);
                if (results?.Count > 0)
                {
                    var primeiroResultado = results[0];
                    if (decimal.TryParse(primeiroResultado.Lat, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal lat) &&
                        decimal.TryParse(primeiroResultado.Lon, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal lon))
                    {
                        Console.WriteLine($"[GEO-LOG] SUCESSO: Coordenadas encontradas: Lat={lat}, Lon={lon}");
                        return (lat, lon);
                    }
                }
            }

            Console.WriteLine("[GEO-LOG] AVISO: Nenhuma das tentativas de geocodificação retornou um resultado válido.");
            return null;
        }
    }
}
