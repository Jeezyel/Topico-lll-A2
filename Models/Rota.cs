using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

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
        [JsonIgnore]
        public Veiculo? Veiculo { get; set; }

        [Required]
        public int MotoristaId { get; set; }
        [ForeignKey("MotoristaId")]
        [JsonIgnore]
        public Motorista? Motorista { get; set; }

        public DateTime DataRota { get; set; } = DateTime.Now;
        public StatusRota Status { get; set; } = StatusRota.Planejada;

        public ICollection<RotaPedido>? RotaPedidos { get; set; }
        public ICollection<AlertaClimatico>? AlertasClimaticos { get; set; }
        public ICollection<IncidenciaRota>? Incidencias { get; set; }
    }
}
