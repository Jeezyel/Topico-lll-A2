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
    public class RotaPedidoController : ControllerBase
    {
        private readonly A2Context _context;

        public RotaPedidoController(A2Context context)
        {
            _context = context;
        }

        // GET: api/RotaPedido
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RotaPedido>>> GetRotaPedidos()
        {
            // Inclui as entidades Rota e Pedido nas consultas
            return await _context.RotaPedidos.Include(rp => rp.Rota).Include(rp => rp.Pedido).ToListAsync();
        }

        // GET: api/RotaPedido/{rotaId}/{pedidoId}
        [HttpGet("{rotaId}/{pedidoId}")]
        public async Task<ActionResult<RotaPedido>> GetRotaPedido(int rotaId, int pedidoId)
        {
            // Busca um RotaPedido com chave composta (RotaId e PedidoId)
            var rotaPedido = await _context.RotaPedidos
                                           .Include(rp => rp.Rota)
                                           .Include(rp => rp.Pedido)
                                           .FirstOrDefaultAsync(rp => rp.RotaId == rotaId && rp.PedidoId == pedidoId);

            if (rotaPedido == null)
                return NotFound();

            return rotaPedido;
        }

        // POST: api/RotaPedido
        [HttpPost]
        public async Task<ActionResult<RotaPedido>> PostRotaPedido(RotaPedido rotaPedido)
        {
            _context.RotaPedidos.Add(rotaPedido);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetRotaPedido", new { rotaId = rotaPedido.RotaId, pedidoId = rotaPedido.PedidoId }, rotaPedido);
        }

        // PUT: api/RotaPedido/{rotaId}/{pedidoId}
        [HttpPut("{rotaId}/{pedidoId}")]
        public async Task<IActionResult> PutRotaPedido(int rotaId, int pedidoId, RotaPedido rotaPedido)
        {
            if (rotaId != rotaPedido.RotaId || pedidoId != rotaPedido.PedidoId)
                return BadRequest();

            _context.Entry(rotaPedido).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.RotaPedidos.Any(rp => rp.RotaId == rotaId && rp.PedidoId == pedidoId))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/RotaPedido/{rotaId}/{pedidoId}
        [HttpDelete("{rotaId}/{pedidoId}")]
        public async Task<IActionResult> DeleteRotaPedido(int rotaId, int pedidoId)
        {
            var rotaPedido = await _context.RotaPedidos.FindAsync(rotaId, pedidoId);
            if (rotaPedido == null)
                return NotFound();

            _context.RotaPedidos.Remove(rotaPedido);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
