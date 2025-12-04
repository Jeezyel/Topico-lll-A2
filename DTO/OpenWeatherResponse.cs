using System.Text.Json.Serialization;

namespace A2.DTO
{
    public class OpenWeatherResponse
    {
        // A API retorna uma lista de condições climáticas
        [JsonPropertyName("weather")]
        public List<WeatherInfo> Weather { get; set; }

        [JsonPropertyName("main")]
        public MainInfo Main { get; set; }
    }
}
