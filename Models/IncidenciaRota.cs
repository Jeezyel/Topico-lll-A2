using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace A2.Models
{
    public class IncidenciaRota
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RotaId { get; set; } // FK

        [Required]
        public int MotoristaId { get; set; } // FK

        [Required]
        public DateTime DataHora { get; set; }

        [Required]
        [StringLength(500)]
        public string Descricao { get; set; }

        // Navegação
        [ForeignKey(nameof(RotaId))]
        public Rota? Rota { get; set; }

        [ForeignKey(nameof(MotoristaId))]
        public Motorista? Motorista { get; set; }
    }
}
