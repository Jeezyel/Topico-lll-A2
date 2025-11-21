using A2.Models;
using A2.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace A2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RotaController : ControllerBase
    {
        private readonly A2Context _context;

        public RotaController(A2Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rota>>> GetRotas()
        {
            return await _context.Rotas
                .Include(r => r.Veiculo)
                .Include(r => r.Motorista)
                .Include(r => r.RotaPedidos)
                    .ThenInclude(rp => rp.Pedido)
                // -------------------------------
                .ToListAsync();
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

            return CreatedAtAction("GetRotas", new { id = rota.Id }, rota);
        }
    }

    public class RotaRequest
    {
        public int VeiculoId { get; set; }
        public int MotoristaId { get; set; }
        public List<int> PedidosIds { get; set; } = new List<int>();
    }
}