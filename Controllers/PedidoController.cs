using A2.Data;
using A2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using A2.DTO;
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
        public async Task<ActionResult<Pedido>> PostPedido(PedidoCreateDto pedidoDto)
        {
            // Validate ClienteId
            if (!await _context.Clientes.AnyAsync(c => c.Id == pedidoDto.ClienteId))
            {
                return BadRequest("Cliente não encontrado.");
            }

            // Validate EnderecoEntregaId
            if (!await _context.EnderecosClientes.AnyAsync(e => e.Id == pedidoDto.EnderecoEntregaId))
            {
                return BadRequest("Endereço de entrega não encontrado.");
            }

            var pedido = new Pedido
            {
                ClienteId = pedidoDto.ClienteId,
                EnderecoEntregaId = pedidoDto.EnderecoEntregaId,
                DataLimiteEntrega = pedidoDto.DataLimiteEntrega,
                Status = StatusPedido.Pendente,
                DataCriacao = DateTime.UtcNow,
                PesoTotalKg = 0,
                VolumeTotalM3 = 0
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync(); // Save to get Pedido.Id

            decimal pesoTotal = 0;
            decimal volumeTotal = 0;

            if (pedidoDto.ItensPedidoIds != null && pedidoDto.ItensPedidoIds.Any())
            {
                var itensParaAssociar = await _context.ItensPedido
                    .Where(i => pedidoDto.ItensPedidoIds.Contains(i.Id) && i.PedidoId == null)
                    .ToListAsync();

                if (itensParaAssociar.Count != pedidoDto.ItensPedidoIds.Count)
                {
                    // This could happen if an ID is invalid or an item is already associated.
                    // For simplicity, we'll proceed, but in a real app, you might want to return a BadRequest.
                }

                foreach (var item in itensParaAssociar)
                {
                    item.PedidoId = pedido.Id;
                    pesoTotal += item.PesoUnitarioKg * (item.Quantidade ?? 1);
                    volumeTotal += item.VolumeUnitarioM3 * (item.Quantidade ?? 1);
                    _context.Entry(item).State = EntityState.Modified;
                }
            }

            pedido.PesoTotalKg = pesoTotal;
            pedido.VolumeTotalM3 = volumeTotal;
            _context.Entry(pedido).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPedido", new { id = pedido.Id }, pedido);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPedido(int id, Pedido pedido)
        {
            if (id != pedido.Id)
            {
                return BadRequest("O ID na URL não corresponde ao ID do pedido fornecido.");
            }

            var existingPedido = await _context.Pedidos
                                                .Include(p => p.ItensPedido)
                                                .AsNoTracking() // Use AsNoTracking para evitar problemas de tracking ao anexar 'pedido'
                                                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingPedido == null)
            {
                return NotFound($"Pedido com ID {id} não encontrado.");
            }

            // Status check (retaining original business logic)
            if (existingPedido.Status != StatusPedido.Pendente)
            {
                return BadRequest("Não é possível alterar um pedido que já está em Rota ou foi Entregue.");
            }

            // Validate ClienteId if it's being changed
            if (existingPedido.ClienteId != pedido.ClienteId && !await _context.Clientes.AnyAsync(c => c.Id == pedido.ClienteId))
            {
                return BadRequest("Novo Cliente associado não encontrado.");
            }

            // Validate EnderecoEntregaId if it's being changed
            if (existingPedido.EnderecoEntregaId != pedido.EnderecoEntregaId && !await _context.EnderecosClientes.AnyAsync(e => e.Id == pedido.EnderecoEntregaId))
            {
                return BadRequest("Novo Endereço de entrega associado não encontrado.");
            }

            // Re-fetch the entity to track it, but apply changes from 'pedido'
            var trackedPedido = await _context.Pedidos
                                            .Include(p => p.ItensPedido)
                                            .FirstOrDefaultAsync(p => p.Id == id);

            if (trackedPedido == null) return NotFound(); // Should not happen given existingPedido != null

            _context.Entry(trackedPedido).CurrentValues.SetValues(pedido);

            // --- Lógica para atualização de ItensPedido ---
            // Remove itens antigos que não estão na nova lista
            foreach (var existingItem in trackedPedido.ItensPedido.ToList())
            {
                if (!pedido.ItensPedido.Any(newItem => newItem.Id == existingItem.Id))
                {
                    _context.ItensPedido.Remove(existingItem);
                }
            }

            // Atualiza itens existentes e adiciona novos itens
            foreach (var newItem in pedido.ItensPedido)
            {
                var existingItem = trackedPedido.ItensPedido.FirstOrDefault(i => i.Id == newItem.Id);
                if (existingItem != null)
                {
                    // Atualiza item existente
                    _context.Entry(existingItem).CurrentValues.SetValues(newItem);
                }
                else
                {
                    // Adiciona novo item
                    newItem.PedidoId = trackedPedido.Id; // Garante que o PedidoId está correto
                    trackedPedido.ItensPedido.Add(newItem);
                }
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
        }
        private bool PedidoExists(int id)
        {
            return _context.Pedidos.Any(e => e.Id == id);
        }
    }
}