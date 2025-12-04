using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using A2.Models;

namespace A2.DTO
{
    public class PedidoUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int EnderecoEntregaId { get; set; }

        [Required]
        public System.DateTime DataLimiteEntrega { get; set; }

        [Required]
        public StatusPedido Status { get; set; }

        public List<int> ItensPedidoIds { get; set; } = new List<int>();
    }
}
