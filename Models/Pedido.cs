using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace A2.Models
{
    public enum StatusPedido
    {
        Pendente, EmRota, Entregue, Cancelado
    }

    public class Pedido
    {
        [Key] public int Id { get; set; }
        [Required] public int ClienteId { get; set; }
        [ForeignKey("ClienteId")] public Cliente? Cliente { get; set; }
        [Required] public int EnderecoEntregaId { get; set; }
        [ForeignKey("EnderecoEntregaId")] public EnderecoCliente? EnderecoEntrega { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime DataLimiteEntrega { get; set; }
        public StatusPedido Status { get; set; } = StatusPedido.Pendente;

        [Column(TypeName = "decimal(10,2)")] public decimal PesoTotalKg { get; set; } = 0;
        [Column(TypeName = "decimal(10,2)")] public decimal VolumeTotalM3 { get; set; } = 0;

        public ICollection<ItemPedido>? ItensPedido { get; set; }

        public ICollection<RotaPedido>? RotaPedidos { get; set; }
    }
}