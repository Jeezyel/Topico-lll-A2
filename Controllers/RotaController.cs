using A2.Data;
using A2.Models;
using A2.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace A2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
                    pedidoComEndereco.EnderecoEntrega.Longitude
);
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