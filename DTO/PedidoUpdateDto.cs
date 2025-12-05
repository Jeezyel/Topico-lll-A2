using A2.Models;
using System.Collections.Generic;

namespace A2.DTO
{
    public class PedidoUpdateDto
    {
        public int ClienteId { get; set; }
        public int EnderecoEntregaId { get; set; }
        public DateTime DataLimiteEntrega { get; set; }
        public StatusPedido Status { get; set; }
        public List<int>? ItensPedidoIds { get; set; }
    }
}