using A2.Data;
using A2.Models;
using A2.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; 

namespace A2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RotaController : ControllerBase
    {
        private readonly A2Context _context;
        private readonly IWeatherService _weatherService;

        public RotaController(A2Context context, IWeatherService weatherService)
        {
            _context = context;
            _weatherService = weatherService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rota>>> GetRotas()
        {
            return await _context.Rotas
                .Include(r => r.Veiculo)
                .Include(r => r.Motorista)
                .Include(r => r.RotaPedidos)
                    .ThenInclude(rp => rp.Pedido)
                .ToListAsync();
        }

        [HttpGet("minhas-rotas")]
        [Authorize(Roles = "Motorista")] // Apenas motoristas podem acessar
        public async Task<ActionResult<IEnumerable<Rota>>> GetMinhasRotas()
        {
            // 1. Obter o ID do usuário logado a partir do token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Unauthorized("Não foi possível identificar o usuário.");
            }
            var userId = int.Parse(userIdStr);

            // 2. Encontrar o motorista associado a esse usuário
            var motorista = await _context.Motoristas.FirstOrDefaultAsync(m => m.UsuarioId == userId);
            if (motorista == null)
            {
                return NotFound("Nenhum perfil de motorista encontrado para este usuário.");
            }

            // 3. Buscar as rotas apenas para este motorista
            return await _context.Rotas
                .Where(r => r.MotoristaId == motorista.Id)
                .Include(r => r.Veiculo)
                .Include(r => r.Motorista)
                .Include(r => r.RotaPedidos)
                    .ThenInclude(rp => rp.Pedido)
                        .ThenInclude(p => p.Cliente)
                .Include(r => r.AlertasClimaticos)
                .Include(r => r.Incidencias)
                .OrderByDescending(r => r.DataRota)
                .ToListAsync();
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Rota>> GetRota(int id)
        {
            var rota = await _context.Rotas
                .Include(r => r.Veiculo)
                .Include(r => r.Motorista)
                .Include(r => r.RotaPedidos)
                    .ThenInclude(rp => rp.Pedido)
                .Include(r => r.AlertasClimaticos)
                .Include(r => r.Incidencias)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rota == null)
            {
                return NotFound();
            }

            return rota;
        }

        [HttpPost]
        public async Task<ActionResult<Rota>> PostRota(RotaRequest request)
        {
            var veiculo = await _context.Veiculos.FindAsync(request.VeiculoId);
            if (veiculo == null) return BadRequest("Veículo inválido.");

            if (veiculo.DataProximaManutencao < DateTime.Today)
                return BadRequest("Veículo está com a manutenção atrasada.");

            if (veiculo.Status != StatusVeiculo.Disponivel)
                return BadRequest("Veículo não está disponível.");

            var motorista = await _context.Motoristas.FindAsync(request.MotoristaId);
            if (motorista == null) return BadRequest("Motorista inválido.");

            var pedidos = await _context.Pedidos
                .Where(p => request.PedidosIds.Contains(p.Id) && p.Status == StatusPedido.Pendente)
                .ToListAsync();

            if (pedidos.Count != request.PedidosIds.Count)
                return BadRequest("Alguns pedidos não foram encontrados ou não estão pendentes.");

            decimal pesoTotal = pedidos.Sum(p => p.PesoTotalKg);
            decimal volumeTotal = pedidos.Sum(p => p.VolumeTotalM3);

            if (pesoTotal > veiculo.CapacidadeCarga)
                return BadRequest($"Peso excedido! Carga: {pesoTotal}kg. Veículo suporta: {veiculo.CapacidadeCarga}kg.");

            if (volumeTotal > veiculo.CapacidadeVolume)
                return BadRequest($"Volume excedido! Carga: {volumeTotal}m3. Veículo suporta: {veiculo.CapacidadeVolume}m3.");

            var rota = new Rota
            {
                VeiculoId = request.VeiculoId,
                MotoristaId = request.MotoristaId,
                Status = StatusRota.Planejada,
                DataRota = DateTime.Now
            };

            _context.Rotas.Add(rota);
            await _context.SaveChangesAsync(); 


            int ordemContador = 1;

            foreach (var p in pedidos)
            {
                var rotaPedido = new RotaPedido
                {
                    RotaId = rota.Id,
                    PedidoId = p.Id,
                    OrdemEntrega = ordemContador++,
                    StatusEntrega = "Pendente"
                };
                _context.RotaPedidos.Add(rotaPedido);

                p.Status = StatusPedido.EmRota;
                _context.Entry(p).State = EntityState.Modified;
            }
            veiculo.Status = StatusVeiculo.EmRota;
            _context.Entry(veiculo).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            var primeiroPedidoId = request.PedidosIds.FirstOrDefault();
            if (primeiroPedidoId != 0)
            {
                // Busca o endereço de entrega deste pedido (precisa do Include para trazer os dados)
                var pedidoComEndereco = await _context.Pedidos
                    .Include(p => p.EnderecoEntrega)
                    .AsNoTracking() // Boa prática para leitura
                    .FirstOrDefaultAsync(p => p.Id == primeiroPedidoId);

                // Verifica se o endereço existe e tem coordenadas válidas
                if (pedidoComEndereco?.EnderecoEntrega != null &&
                    pedidoComEndereco.EnderecoEntrega.Latitude != 0 &&
                    pedidoComEndereco.EnderecoEntrega.Longitude != 0)
                {
                    var alerta = await _weatherService.VerificarClimaAsync(
                    pedidoComEndereco.EnderecoEntrega.Latitude,
                    pedidoComEndereco.EnderecoEntrega.Longitude);

                    if (alerta != null)
                    {
                        alerta.RotaId = rota.Id;
                        _context.AlertasClimaticos.Add(alerta);
                        await _context.SaveChangesAsync();

                        if (rota.AlertasClimaticos == null) rota.AlertasClimaticos = new List<AlertaClimatico>();
                        rota.AlertasClimaticos.Add(alerta);
                    }
                }
            }

            return CreatedAtAction("GetRota", new { id = rota.Id }, rota);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRota(int id, Rota rota)
        {
            if (id != rota.Id)
            {
                return BadRequest("O ID na URL não corresponde ao ID da rota fornecida.");
            }

            var existingRota = await _context.Rotas
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(r => r.Id == id);

            if (existingRota == null)
            {
                return NotFound($"Rota com ID {id} não encontrada.");
            }

            // Validar se o status da rota permite alteração
            if (existingRota.Status != StatusRota.Planejada)
            {
                return BadRequest("Não é possível alterar uma rota que não está 'Planejada'.");
            }
            
            // Validar VeiculoId e MotoristaId
            if (!await _context.Veiculos.AnyAsync(v => v.Id == rota.VeiculoId))
            {
                return BadRequest("Veículo associado não encontrado.");
            }
            if (!await _context.Motoristas.AnyAsync(m => m.Id == rota.MotoristaId))
            {
                return BadRequest("Motorista associado não encontrado.");
            }

            // Anexar a entidade e marcar como modificada
            _context.Entry(rota).State = EntityState.Modified;
            
            // Garante que não se tente alterar as coleções diretamente por aqui,
            // que devem ser gerenciadas por endpoints específicos (ex: adicionar/remover pedidos da rota)
            _context.Entry(rota).Property(r => r.RotaPedidos).IsModified = false;
            _context.Entry(rota).Property(r => r.AlertasClimaticos).IsModified = false;


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RotaExists(id))
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
        public async Task<IActionResult> DeleteRota(int id)
        {
            var rota = await _context.Rotas.FindAsync(id);
            if (rota == null)
            {
                return NotFound();
            }

            if (rota.Status == StatusRota.EmAndamento)
            {
                return BadRequest("Não é possível excluir uma rota que está em andamento.");
            }

            _context.Rotas.Remove(rota);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{rotaId}/pedidos/{pedidoId}/entregar")]
        public async Task<IActionResult> MarcarPedidoComoEntregue(int rotaId, int pedidoId)
        {
            var rotaPedido = await _context.RotaPedidos
                .FirstOrDefaultAsync(rp => rp.RotaId == rotaId && rp.PedidoId == pedidoId);

            if (rotaPedido == null)
            {
                return NotFound("O vínculo entre esta rota e este pedido não foi encontrado.");
            }

            if (rotaPedido.StatusEntrega == "Entregue")
            {
                return BadRequest("Este pedido já está marcado como entregue nesta rota.");
            }

            rotaPedido.StatusEntrega = "Entregue";

            var pedidoPrincipal = await _context.Pedidos.FindAsync(pedidoId);
            if (pedidoPrincipal != null)
            {
                pedidoPrincipal.Status = StatusPedido.Entregue;
                _context.Entry(pedidoPrincipal).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            bool existemPendencias = await _context.RotaPedidos
                .AnyAsync(rp => rp.RotaId == rotaId && rp.StatusEntrega != "Entregue");

            if (!existemPendencias)
            {
                var rota = await _context.Rotas
                    .Include(r => r.Veiculo)
                    .FirstOrDefaultAsync(r => r.Id == rotaId);

                if (rota != null)
                {
                    rota.Status = StatusRota.Concluida;
                    _context.Entry(rota).State = EntityState.Modified;

                    if (rota.Veiculo != null)
                    {
                        rota.Veiculo.Status = StatusVeiculo.Disponivel;
                        _context.Entry(rota.Veiculo).State = EntityState.Modified;
                    }

                    await _context.SaveChangesAsync();
                }
            }

            // 3. O ERRO CS0161 ERA PORQUE FALTAVA ESTA LINHA NO FINAL:
            return NoContent();
        }

        private bool RotaExists(int id)
        {
            return _context.Rotas.Any(e => e.Id == id);
        }
    }

    public class RotaRequest
    {
        public int VeiculoId { get; set; }
        public int MotoristaId { get; set; }
        public List<int> PedidosIds { get; set; } = new List<int>();
    }
}