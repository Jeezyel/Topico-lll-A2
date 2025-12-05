using A2.Data;
using A2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace A2.Controllers
{
    [Route("api/cliente-portal")]
    [ApiController]
    [Authorize(Roles = "Cliente")]
    public class ClientePortalController : ControllerBase
    {
        private readonly A2Context _context;

        public ClientePortalController(A2Context context)
        {
            _context = context;
        }

        [HttpGet("meus-pedidos")]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetMeusPedidos()
        {
            // 1. Get the logged-in user's ID from the token claims
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized("Invalid user identifier.");
            }

            // 2. Find the Cliente profile associated with this user ID
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == userId);
            if (cliente == null)
            {
                return NotFound("No client profile found for the logged-in user.");
            }

            // 3. Retrieve all orders for this client, including related route info
            var pedidos = await _context.Pedidos
                .Where(p => p.ClienteId == cliente.Id)
                .Include(p => p.ItensPedido)
                .Include(p => p.RotaPedidos)
                    .ThenInclude(rp => rp.Rota)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();

            return Ok(pedidos);
        }
    }
}
