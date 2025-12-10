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
        private readonly ILogger<PedidoController> _logger;

        public PedidoController(A2Context context, ILogger<PedidoController> logger)
        {
            _context = context;
            _logger = logger;
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

        [HttpGet("quantidade")] // Define uma rota específica, ex: /api/clientes/quantidade
        public async Task<ActionResult<int>> GetQuantidadePedidos()
        {
            // O CountAsync é mais eficiente pois executa a contagem direto no banco
            var quantidade = await _context.Pedidos.CountAsync();

            return Ok(quantidade);
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
                _logger.LogWarning("Não foi possível encontrar o NameIdentifier (ID do usuário) no token.");
                return Unauthorized("Não foi possível identificar o usuário.");
            }
            var userId = int.Parse(userIdStr);
            _logger.LogInformation("Buscando pedidos para o usuário com ID: {UserId}", userId);


            // 2. Encontrar o cliente associado a esse usuário
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == userId);
            if (cliente == null)
            {
                _logger.LogWarning("Nenhum perfil de cliente encontrado para o usuário com ID: {UserId}", userId);
                return NotFound("Nenhum perfil de cliente encontrado para este usuário.");
            }
            _logger.LogInformation("Cliente encontrado com ID: {ClienteId} para o usuário com ID: {UserId}", cliente.Id, userId);

            // 3. Buscar os pedidos apenas para este cliente
            var pedidos = await _context.Pedidos
                .Where(p => p.ClienteId == cliente.Id)
                .Include(p => p.EnderecoEntrega)
                .Include(p => p.ItensPedido)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();
            
            _logger.LogInformation("Encontrados {NumPedidos} pedidos para o ClienteId {ClienteId}", pedidos.Count, cliente.Id);

            return pedidos;
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
        public async Task<IActionResult> PutPedido(int id, PedidoUpdateDto pedidoDto)
        {
            var trackedPedido = await _context.Pedidos
                                            .Include(p => p.ItensPedido)
                                            .FirstOrDefaultAsync(p => p.Id == id);

            if (trackedPedido == null)
            {
                return NotFound($"Pedido com ID {id} não encontrado.");
            }

            // Regra de negócio: Só permite edição de pedidos 'Pendentes' ou reabertura de 'Cancelados'.
            if (trackedPedido.Status != StatusPedido.Pendente)
            {
                // A única exceção é reabrir um pedido cancelado, mudando seu status para pendente.
                if(trackedPedido.Status == StatusPedido.Cancelado && pedidoDto.Status == StatusPedido.Pendente)
                {
                    // Permite a reabertura.
                }
                else
                {
                    return BadRequest("Não é possível alterar um pedido que não está com o status 'Pendente'.");
                }
            }
            
            // Impede a mudança manual para status que são controlados por outros processos (Rotas).
            if (pedidoDto.Status != StatusPedido.Pendente && pedidoDto.Status != StatusPedido.Cancelado && pedidoDto.Status != StatusPedido.EmRota)
            {
                return BadRequest("O status de um pedido só pode ser alterado para 'Pendente', 'Cancelado' ou 'EmRota' através desta função.");
            }

            // Valida o ClienteId se ele estiver sendo alterado
            if (trackedPedido.ClienteId != pedidoDto.ClienteId && !await _context.Clientes.AnyAsync(c => c.Id == pedidoDto.ClienteId))
            {
                return BadRequest("Novo Cliente associado não encontrado.");
            }

            // Valida o EnderecoEntregaId se ele estiver sendo alterado
            if (trackedPedido.EnderecoEntregaId != pedidoDto.EnderecoEntregaId && !await _context.EnderecosClientes.AnyAsync(e => e.Id == pedidoDto.EnderecoEntregaId))
            {
                return BadRequest("Novo Endereço de entrega associado não encontrado.");
            }

            // Atualiza as propriedades do pedido com os dados do DTO
            trackedPedido.ClienteId = pedidoDto.ClienteId;
            trackedPedido.EnderecoEntregaId = pedidoDto.EnderecoEntregaId;
            trackedPedido.DataLimiteEntrega = pedidoDto.DataLimiteEntrega;
            trackedPedido.Status = pedidoDto.Status;

            // Atualiza a coleção de ItensPedido
            var newItensIds = pedidoDto.ItensPedidoIds ?? new List<int>();
            var existingItensIds = trackedPedido.ItensPedido.Select(i => i.Id).ToList();

            var itensToRemove = trackedPedido.ItensPedido.Where(i => !newItensIds.Contains(i.Id)).ToList();
            var itensToAddIds = newItensIds.Where(id => !existingItensIds.Contains(id)).ToList();

            if (itensToAddIds.Any())
            {
                var itensToAdd = await _context.ItensPedido.Where(i => itensToAddIds.Contains(i.Id) && i.PedidoId == null).ToListAsync();
                if (itensToAdd.Count != itensToAddIds.Count)
                {
                    return BadRequest("Um ou mais IDs de novos itens são inválidos ou já pertencem a outro pedido.");
                }
                foreach (var item in itensToAdd)
                {
                    trackedPedido.ItensPedido.Add(item);
                }
            }

            if (itensToRemove.Any())
            {
                foreach(var item in itensToRemove)
                {
                    // Desassocia o item em vez de remover, caso ele possa ser usado em outro lugar.
                    item.PedidoId = null;
                    _context.Entry(item).State = EntityState.Modified;
                }
            }
            
            // Recalcula o peso e volume totais
            decimal pesoTotal = 0;
            decimal volumeTotal = 0;
            foreach (var item in trackedPedido.ItensPedido)
            {
                pesoTotal += item.PesoUnitarioKg * (item.Quantidade ?? 1);
                volumeTotal += item.VolumeUnitarioM3 * (item.Quantidade ?? 1);
            }
            trackedPedido.PesoTotalKg = pesoTotal;
            trackedPedido.VolumeTotalM3 = volumeTotal;

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
            var pedido = await _context.Pedidos
                .Include(p => p.ItensPedido)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
            {
                return NotFound();
            }

            if (pedido.Status == StatusPedido.EmRota || pedido.Status == StatusPedido.Entregue)
            {
                return BadRequest("Não é possível excluir um pedido que está 'Em Rota' ou já foi 'Entregue'.");
            }

            try
            {
                // 1. Remove associações em RotaPedido
                var rotaPedidos = await _context.RotaPedidos.Where(rp => rp.PedidoId == id).ToListAsync();
                if (rotaPedidos.Any())
                {
                    _context.RotaPedidos.RemoveRange(rotaPedidos);
                }

                // 2. Desassocia Itens do Pedido (para que não sejam órfãos ou excluídos)
                if (pedido.ItensPedido != null && pedido.ItensPedido.Any())
                {
                    foreach (var item in pedido.ItensPedido)
                    {
                        item.PedidoId = null;
                        _context.Entry(item).State = EntityState.Modified;
                    }
                }

                // 3. Remove o Pedido
                _context.Pedidos.Remove(pedido);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro de banco de dados ao tentar excluir o pedido {PedidoId}", id);
                return Conflict("Não foi possível excluir o pedido devido a outras dependências no banco de dados. Verifique logs para mais detalhes.");
            }

            return NoContent();
        }
        private bool PedidoExists(int id)
        {
            return _context.Pedidos.Any(e => e.Id == id);
        }
    }
}