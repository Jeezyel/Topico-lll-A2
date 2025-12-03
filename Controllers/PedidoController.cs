using A2.Data;
using A2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace A2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PedidoController : ControllerBase
    {
        private readonly A2Context _context;

        public PedidoController(A2Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.EnderecoEntrega)
                .Include(p => p.ItensPedido)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Pedido>> GetPedido(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.EnderecoEntrega)
                .Include(p => p.ItensPedido)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();

            return pedido;
        }

        [HttpGet("pendentes")]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidosPendentes()
        {
            return await _context.Pedidos
                .Where(p => p.Status == StatusPedido.Pendente)
                .Include(p => p.Cliente)
                .Include(p => p.EnderecoEntrega)
                .ToListAsync();
        }

        [HttpGet("meus-pedidos")]
        [Authorize(Roles = "Cliente")] // Apenas clientes podem acessar
        public async Task<ActionResult<IEnumerable<Pedido>>> GetMeusPedidos()
        {
            // 1. Obter o ID do usuário logado
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Unauthorized("Não foi possível identificar o usuário.");
            }
            var userId = int.Parse(userIdStr);

            // 2. Encontrar o cliente associado a esse usuário
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == userId);
            if (cliente == null)
            {
                return NotFound("Nenhum perfil de cliente encontrado para este usuário.");
            }

            // 3. Buscar os pedidos apenas para este cliente
            return await _context.Pedidos
                .Where(p => p.ClienteId == cliente.Id)
                .Include(p => p.EnderecoEntrega)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Pedido>> PostPedido(A2.DTO.PedidoCreateDto pedidoDto)
        {
            if (!await _context.Clientes.AnyAsync(c => c.Id == pedidoDto.ClienteId))
            {
                return BadRequest("Cliente não encontrado.");
            }

            if (!await _context.EnderecosClientes.AnyAsync(e => e.Id == pedidoDto.EnderecoEntregaId))
            {
                return BadRequest("Endereço de entrega não encontrado.");
            }

            var itens = await _context.ItensPedido
                                      .Where(i => pedidoDto.ItensPedidoIds.Contains(i.Id))
                                      .ToListAsync();

            if (itens.Count != pedidoDto.ItensPedidoIds.Count)
            {
                return BadRequest("Um ou mais itens do pedido não foram encontrados.");
            }
            
            var pedido = new Pedido
            {
                ClienteId = pedidoDto.ClienteId,
                EnderecoEntregaId = pedidoDto.EnderecoEntregaId,
                DataLimiteEntrega = pedidoDto.DataLimiteEntrega,
                Status = StatusPedido.Pendente,
                ItensPedido = itens
            };

            pedido.PesoTotalKg = pedido.ItensPedido.Sum(i => i.PesoUnitarioKg * i.Quantidade);
            pedido.VolumeTotalM3 = pedido.ItensPedido.Sum(i => i.VolumeUnitarioM3 * i.Quantidade);

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPedido", new { id = pedido.Id }, pedido);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPedido(int id, A2.DTO.PedidoUpdateDto pedidoDto)
        {
            if (id != pedidoDto.Id)
            {
                return BadRequest("O ID na URL não corresponde ao ID do pedido fornecido.");
            }

            var trackedPedido = await _context.Pedidos
                                            .Include(p => p.ItensPedido)
                                            .FirstOrDefaultAsync(p => p.Id == id);
            
            if(trackedPedido == null) return NotFound();

            if (trackedPedido.Status != StatusPedido.Pendente)
            {
                return BadRequest("Não é possível alterar um pedido que já está em Rota ou foi Entregue.");
            }
            
            _context.Entry(trackedPedido).CurrentValues.SetValues(pedidoDto);

            var novosItens = await _context.ItensPedido
                                           .Where(i => pedidoDto.ItensPedidoIds.Contains(i.Id))
                                           .ToListAsync();

            if (novosItens.Count != pedidoDto.ItensPedidoIds.Count)
            {
                return BadRequest("Um ou mais itens do pedido não foram encontrados.");
            }

            trackedPedido.ItensPedido = novosItens;

            if (trackedPedido.ItensPedido != null && trackedPedido.ItensPedido.Any())
            {
                trackedPedido.PesoTotalKg = trackedPedido.ItensPedido.Sum(i => i.PesoUnitarioKg * i.Quantidade);
                trackedPedido.VolumeTotalM3 = trackedPedido.ItensPedido.Sum(i => i.VolumeUnitarioM3 * i.Quantidade);
            }
            else
            {
                trackedPedido.PesoTotalKg = 0;
                trackedPedido.VolumeTotalM3 = 0;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PedidoExists(id))
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
        public async Task<IActionResult> DeletePedido(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            if (pedido.Status != StatusPedido.Pendente && pedido.Status != StatusPedido.Cancelado)
            {
                return BadRequest("Não é possível excluir um pedido que está ativo em uma Rota.");
            }

            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();

            return NoContent();
        }        private bool PedidoExists(int id)
        {
            return _context.Pedidos.Any(e => e.Id == id);
        }
    }
}
