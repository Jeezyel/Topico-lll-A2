using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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
        [JsonIgnore]
        [ForeignKey("VeiculoId")]
        public Veiculo? Veiculo { get; set; }

        [Required]
        public int MotoristaId { get; set; }
        [JsonIgnore]
        [ForeignKey("MotoristaId")]
        public Motorista? Motorista { get; set; }

        public DateTime DataRota { get; set; } = DateTime.Now;
        public StatusRota Status { get; set; } = StatusRota.Planejada;

        // Lista de pedidos nesta rota
        [JsonIgnore]
        public ICollection<Pedido>? Pedidos { get; set; }
    }
}
