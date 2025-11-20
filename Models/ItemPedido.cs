using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace A2.Models
{
    public class ItemPedido
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PedidoId { get; set; }
        [JsonIgnore]
        [ForeignKey("PedidoId")]
        public Pedido? Pedido { get; set; }

        [Required]
        [StringLength(200)]
        public string Descricao { get; set; }

        [Required]
        public int Quantidade { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PesoUnitarioKg { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal VolumeUnitarioM3 { get; set; }

        [StringLength(100)]
        public string? CodigoProduto { get; set; }
    }
}
