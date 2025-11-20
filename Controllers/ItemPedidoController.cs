using A2.Models;
using A2.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace A2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemPedidoController : ControllerBase
    {
        private readonly A2Context _context;

        public ItemPedidoController(A2Context context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<ItemPedido>> PostItem(ItemPedido item)
        {
            var pedido = await _context.Pedidos.FindAsync(item.PedidoId);
            if (pedido == null) return NotFound("Pedido não encontrado.");

            _context.ItensPedido.Add(item);

            pedido.PesoTotalKg += (item.PesoUnitarioKg * item.Quantidade);
            pedido.VolumeTotalM3 += (item.VolumeUnitarioM3 * item.Quantidade);

            _context.Entry(pedido).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok(item);
        }
    }
}
