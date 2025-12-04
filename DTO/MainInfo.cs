using System.Text.Json.Serialization;

namespace A2.DTO
{
    public class MainInfo
    {
        [JsonPropertyName("temp")]
        public double Temp { get; set; }

        [JsonPropertyName("feels_like")]
        public double FeelsLike { get; set; }
    }
}
