using System.Text.Json.Serialization;

namespace A2.DTO
{
    public class WeatherInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        // A descrição (ex: "chuva moderada", "céu limpo")
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }
    }
}
