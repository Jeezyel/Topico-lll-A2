using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace A2.Models
{
    public class WeatherForces
    {
        // 🔑 Chave Primária
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // Adicionado para servir como chave primária

        // 📝 Descrição do Tempo
        [Required(ErrorMessage = "A Descricao não foi informada")]
        public string Descricao { get; set; }

        // 🌡️ Temperatura em Celsius/Fahrenheit (dependendo da sua aplicação)
        [Required (ErrorMessage = "A Temperatura não foi informada")]
        public double Temperatura { get; set; }

        // 🥶 Sensação Térmica
        [Required (ErrorMessage = "A SensacaoTermica não foi informada")]
        public double SensacaoTermica { get; set; }

        // 🖼️ Nome/Caminho do Ícone (pode ser nullable)
        public string? Icone { get; set; }

        // 🚨 Tipo de Alerta de Tempo (pode ser nullable)
        public string? TipoAlerta { get; set; }

        // ⚠️ Severidade do Alerta (pode ser nullable)
        public string? Severidade { get; set; }

        public decimal? latitude { get; set; }
        public decimal? longitude { get; set; }

    }
}
