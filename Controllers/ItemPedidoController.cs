using A2.Data;
using A2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace A2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ItemPedidoController : ControllerBase
    {
        private readonly A2Context _context;

        public ItemPedidoController(A2Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemPedido>>> GetItensPedido([FromQuery] int? pedidoId, [FromQuery] bool semPedido = false)
        {
            var query = _context.ItensPedido.AsQueryable();

            if (pedidoId.HasValue)
            {
                query = query.Where(i => i.PedidoId == pedidoId.Value);
            }

            if (semPedido)
            {
                query = query.Where(i => i.PedidoId == null);
            }

            return await query.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemPedido>> GetItemPedido(int id)
        {
            var itemPedido = await _context.ItensPedido.FindAsync(id);

            if (itemPedido == null)
            {
                return NotFound();
            }

            return itemPedido;
        }

        [HttpPost]
        public async Task<ActionResult<ItemPedido>> PostItem(ItemPedido item)
        {
            if (item.PedidoId.HasValue)
            {
                var pedido = await _context.Pedidos.FindAsync(item.PedidoId);
                if (pedido == null) return NotFound("Pedido não encontrado.");

                // Recalculate totals
                pedido.PesoTotalKg += (item.PesoUnitarioKg * (item.Quantidade ?? 0));
                pedido.VolumeTotalM3 += (item.VolumeUnitarioM3 * (item.Quantidade ?? 0));
                _context.Entry(pedido).State = EntityState.Modified;
            }

            _context.ItensPedido.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItemPedido), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutItemPedido(int id, ItemPedido itemPedido)
        {
            if (id != itemPedido.Id)
            {
                return BadRequest();
            }

            var originalItem = await _context.ItensPedido.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
            if (originalItem == null) return NotFound();

            var pedido = await _context.Pedidos.FindAsync(itemPedido.PedidoId);
            if (pedido == null) return NotFound("Pedido não encontrado.");

            // Recalculate totals
            pedido.PesoTotalKg -= (originalItem.PesoUnitarioKg * (originalItem.Quantidade ?? 0));
            pedido.VolumeTotalM3 -= (originalItem.VolumeUnitarioM3 * (originalItem.Quantidade ?? 0));
            pedido.PesoTotalKg += (itemPedido.PesoUnitarioKg * (itemPedido.Quantidade ?? 0));
            pedido.VolumeTotalM3 += (itemPedido.VolumeUnitarioM3 * (itemPedido.Quantidade ?? 0));

            _context.Entry(pedido).State = EntityState.Modified;
            _context.Entry(itemPedido).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemPedidoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItemPedido(int id)
        {
            var itemPedido = await _context.ItensPedido.FindAsync(id);
            if (itemPedido == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos.FindAsync(itemPedido.PedidoId);
            if (pedido != null)
            {
                // Recalculate totals
                pedido.PesoTotalKg -= (itemPedido.PesoUnitarioKg * (itemPedido.Quantidade ?? 0));
                pedido.VolumeTotalM3 -= (itemPedido.VolumeUnitarioM3 * (itemPedido.Quantidade ?? 0));
                _context.Entry(pedido).State = EntityState.Modified;
            }

            _context.ItensPedido.Remove(itemPedido);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ItemPedidoExists(int id)
        {
            return _context.ItensPedido.Any(e => e.Id == id);
        }
    }
}