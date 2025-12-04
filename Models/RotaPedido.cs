using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace A2.Models
{
    public class RotaPedido
    {
        // Como EF Core suporta [Key] apenas em uma propriedade,
        // pode-se usar [Key] + [Column(Order = ...)] OU configurar no OnModelCreating.
        // Aqui usaremos o estilo com múltiplas [Key].

        [Required(ErrorMessage = "O ID da Rota é obrigatório.")]
        public int RotaId { get; set; }

        [Required(ErrorMessage = "O ID do Pedido é obrigatório.")]
        public int PedidoId { get; set; }

        [Required]
        public int OrdemEntrega { get; set; }

        [Required]
        [StringLength(50)]
        public string StatusEntrega { get; set; }

        // Navegações
        [ForeignKey(nameof(RotaId))]
        [JsonIgnore]
        public Rota? Rota { get; set; }

        [ForeignKey(nameof(PedidoId))]
        [JsonIgnore]
        public Pedido? Pedido { get; set; }
    }
}
