using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace A2.Models
{
    public enum StatusPedido
    {
        Pendente = 0 , EmRota = 1, Entregue= 2, Cancelado = 3
    }

    public class Pedido
    {
        [Key] public int Id { get; set; }
        [Required] public int ClienteId { get; set; }
        [ForeignKey("ClienteId")] [JsonIgnore] public Cliente? Cliente { get; set; }
        [Required] public int EnderecoEntregaId { get; set; }
        [ForeignKey("EnderecoEntregaId")] [JsonIgnore] public EnderecoCliente? EnderecoEntrega { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime DataLimiteEntrega { get; set; }
        public StatusPedido Status { get; set; } = StatusPedido.Pendente;

        public decimal PesoTotalKg { get; set; } = 0;
        public decimal VolumeTotalM3 { get; set; } = 0;

        public ICollection<ItemPedido>? ItensPedido { get; set; }

        [JsonIgnore]
        public ICollection<RotaPedido>? RotaPedidos { get; set; }
    }
}