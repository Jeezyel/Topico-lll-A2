using System.Text.Json.Serialization;

namespace A2.DTOs
{
    // Esta classe serve apenas para ler a resposta da API do Nominatim.
    // Não será salva no banco.
    public class NominatimResult
    {
        [JsonPropertyName("lat")]
        public string Lat { get; set; } // Nominatim devolve como string

        [JsonPropertyName("lon")]
        public string Lon { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }
    }
}