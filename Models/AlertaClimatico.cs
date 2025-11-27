using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace A2.Models
{
    public class AlertaClimatico
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RotaId { get; set; } // FK

        [Required]
        [StringLength(100)]
        public string TipoAlerta { get; set; }

        [Required]
        [StringLength(50)]
        public string Severidade { get; set; }

        // Navegação
        [ForeignKey(nameof(RotaId))]
        [JsonIgnore]
        public Rota? Rota { get; set; }
    }
}
