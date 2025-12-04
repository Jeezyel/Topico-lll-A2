using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace A2.Models
{
    public class AlertaClimatico
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RotaId { get; set; }

        // Campos do Alerta (opcionais)
        public string? TipoAlerta { get; set; }
        public string? Severidade { get; set; }

        // Novos campos para detalhes do tempo
        [Required]
        public string Descricao { get; set; } = string.Empty;
        public double Temperatura { get; set; }
        public double SensacaoTermica { get; set; }
        public string Icone { get; set; } = string.Empty;


        // Navegação
        [ForeignKey(nameof(RotaId))]
        [JsonIgnore]
        public Rota? Rota { get; set; }
    }
}
