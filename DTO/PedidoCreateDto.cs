// DTO/PedidoCreateDto.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace A2.DTO
{
    public class PedidoCreateDto
    {
        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int EnderecoEntregaId { get; set; }

        [Required]
        public DateTime DataLimiteEntrega { get; set; }

        public List<int> ItensPedidoIds { get; set; } = new List<int>();
    }
}
