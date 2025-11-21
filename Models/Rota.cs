using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace A2.Models
{
    public enum StatusRota
    {
        Planejada, EmAndamento, Concluida, Cancelada
    }
    public class Rota
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VeiculoId { get; set; }
        [ForeignKey("VeiculoId")]
        public Veiculo? Veiculo { get; set; }

        [Required]
        public int MotoristaId { get; set; }
        [ForeignKey("MotoristaId")]
        public Motorista? Motorista { get; set; }

        public DateTime DataRota { get; set; } = DateTime.Now;
        public StatusRota Status { get; set; } = StatusRota.Planejada;

        public ICollection<RotaPedido>? RotaPedidos { get; set; }
    }
}
