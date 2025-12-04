namespace A2.DTO
{
    public class WeatherForecastDto
    {
        public string Descricao { get; set; }
        public double Temperatura { get; set; }
        public double SensacaoTermica { get; set; }
        public string Icone { get; set; }
        public string? TipoAlerta { get; set; }
        public string? Severidade { get; set; }
    }
}
