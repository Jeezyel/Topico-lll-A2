using A2.Data;
﻿using A2.Models;
﻿using A2.Service;
﻿using Microsoft.AspNetCore.Authorization;
﻿using Microsoft.AspNetCore.Mvc;
﻿using Microsoft.EntityFrameworkCore;
﻿using System.Security.Claims;
﻿
﻿namespace A2.Controllers
﻿{
﻿    [Route("api/[controller]")]
﻿    [ApiController]
﻿    [Authorize]
﻿    public class RotaController : ControllerBase
﻿    {
﻿        private readonly A2Context _context;
﻿        private readonly IWeatherService _weatherService;
﻿        private readonly IGeocodingService _geocodingService; // Adicionado
﻿
﻿        public RotaController(A2Context context, IWeatherService weatherService, IGeocodingService geocodingService) // Adicionado
﻿        {
﻿            _context = context;
﻿            _weatherService = weatherService;
﻿            _geocodingService = geocodingService; // Adicionado
﻿        }
﻿
﻿        [HttpGet]
﻿        public async Task<ActionResult<IEnumerable<Rota>>> GetRotas()
﻿        {
﻿            return await _context.Rotas
﻿                .Include(r => r.Veiculo)
﻿                .Include(r => r.Motorista)
﻿                .Include(r => r.RotaPedidos)
﻿                    .ThenInclude(rp => rp.Pedido)
﻿                .Include(r => r.AlertasClimaticos)
﻿                .Include(r => r.Incidencias)
﻿                .ToListAsync();
﻿        }
﻿        [HttpGet("minhas-rotas")]
﻿        [Authorize(Roles = "Motorista")] // Apenas motoristas podem acessar
﻿        public async Task<ActionResult<IEnumerable<Rota>>> GetMinhasRotas()
﻿        {
﻿            // 1. Obter o ID do usuário logado a partir do token
﻿            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
﻿            if (string.IsNullOrEmpty(userIdStr))
﻿            {
﻿                return Unauthorized("Não foi possível identificar o usuário.");
﻿            }
﻿            var userId = int.Parse(userIdStr);
﻿
﻿            // 2. Encontrar o motorista associado a esse usuário
﻿            var motorista = await _context.Motoristas.FirstOrDefaultAsync(m => m.UsuarioId == userId);
﻿            if (motorista == null)
﻿            {
﻿                return NotFound("Nenhum perfil de motorista encontrado para este usuário.");
﻿            }
﻿
﻿            // 3. Buscar as rotas apenas para este motorista
﻿            return await _context.Rotas
﻿                .Where(r => r.MotoristaId == motorista.Id)
﻿                .Include(r => r.Veiculo)
﻿                .Include(r => r.Motorista)
﻿                .Include(r => r.RotaPedidos)
﻿                    .ThenInclude(rp => rp.Pedido)
﻿                        .ThenInclude(p => p.Cliente)
﻿                .Include(r => r.AlertasClimaticos)
﻿                .Include(r => r.Incidencias)
﻿                .OrderByDescending(r => r.DataRota)
﻿                .ToListAsync();
﻿        }
﻿
﻿        [HttpGet("{id}")]
﻿        public async Task<ActionResult<Rota>> GetRota(int id)
﻿        {
﻿            var rota = await _context.Rotas
﻿                .Include(r => r.Veiculo)
﻿                .Include(r => r.Motorista)
﻿                .Include(r => r.RotaPedidos)
﻿                    .ThenInclude(rp => rp.Pedido)
﻿                        .ThenInclude(p => p.Cliente) // Incluindo o Cliente
﻿                .Include(r => r.RotaPedidos)
﻿                    .ThenInclude(rp => rp.Pedido)
﻿                        .ThenInclude(p => p.EnderecoEntrega) // Incluindo o Endereço de Entrega
﻿                .Include(r => r.AlertasClimaticos)
﻿                .Include(r => r.Incidencias)
﻿                .FirstOrDefaultAsync(r => r.Id == id);
﻿
﻿            if (rota == null)
﻿            {
﻿                return NotFound();
﻿            }
﻿
﻿            return rota;
﻿        }
﻿
﻿        [HttpPost]
﻿        public async Task<ActionResult<Rota>> PostRota(RotaRequest request)
﻿        {
﻿            var veiculo = await _context.Veiculos.FindAsync(request.VeiculoId);
﻿            if (veiculo == null) return BadRequest("Veículo inválido.");
﻿
﻿            if (veiculo.DataProximaManutencao < DateTime.Today)
﻿                return BadRequest("Veículo está com a manutenção atrasada.");
﻿
﻿            if (veiculo.Status != StatusVeiculo.Disponivel)
﻿                return BadRequest("Veículo não está disponível.");
﻿
﻿            var motorista = await _context.Motoristas.FindAsync(request.MotoristaId);
﻿            if (motorista == null) return BadRequest("Motorista inválido.");
﻿
﻿            var pedidos = await _context.Pedidos
﻿                .Where(p => request.PedidosIds.Contains(p.Id) && p.Status == StatusPedido.Pendente)
﻿                .ToListAsync();
﻿
﻿            if (pedidos.Count != request.PedidosIds.Count)
﻿                return BadRequest("Alguns pedidos não foram encontrados ou não estão pendentes.");
﻿
﻿            decimal pesoTotal = pedidos.Sum(p => p.PesoTotalKg);
﻿            decimal volumeTotal = pedidos.Sum(p => p.VolumeTotalM3);
﻿
﻿            if (pesoTotal > veiculo.CapacidadeCarga)
﻿                return BadRequest($"Peso excedido! Carga: {pesoTotal}kg. Veículo suporta: {veiculo.CapacidadeCarga}kg.");
﻿
﻿            if (volumeTotal > veiculo.CapacidadeVolume)
﻿                return BadRequest($"Volume excedido! Carga: {volumeTotal}m3. Veículo suporta: {veiculo.CapacidadeVolume}m3.");
﻿
﻿            var rota = new Rota
﻿            {
﻿                VeiculoId = request.VeiculoId,
﻿                MotoristaId = request.MotoristaId,
﻿                Status = StatusRota.Planejada,
﻿                DataRota = DateTime.Now
﻿            };
﻿
﻿            _context.Rotas.Add(rota);
﻿            await _context.SaveChangesAsync(); 
﻿
﻿            int ordemContador = 1;
﻿
﻿            foreach (var p in pedidos)
﻿            {
﻿                var rotaPedido = new RotaPedido
﻿                {
﻿                    RotaId = rota.Id,
﻿                    PedidoId = p.Id,
﻿                    OrdemEntrega = ordemContador++,
﻿                    StatusEntrega = "Pendente"
﻿                };
﻿                _context.RotaPedidos.Add(rotaPedido);
﻿
﻿                p.Status = StatusPedido.EmRota;
﻿                _context.Entry(p).State = EntityState.Modified;
﻿            }
﻿            veiculo.Status = StatusVeiculo.EmRota;
﻿            _context.Entry(veiculo).State = EntityState.Modified;
﻿
﻿            await _context.SaveChangesAsync();
﻿
﻿                        // --- LÓGICA DE GEOLOCALIZAÇÃO E CLIMA REATORADA ---
﻿
﻿                        var primeiroPedidoId = request.PedidosIds.FirstOrDefault();
﻿
﻿                        Console.WriteLine($"[LOG] ID do primeiro pedido da lista: {primeiroPedidoId}");
﻿
﻿            
﻿
﻿                        if (primeiroPedidoId != 0)
﻿
﻿                        {
﻿
﻿                            var pedidoComEndereco = await _context.Pedidos
﻿
﻿                                .Include(p => p.EnderecoEntrega) // Importante: Incluir o endereço para geocodificação
﻿
﻿                                .FirstOrDefaultAsync(p => p.Id == primeiroPedidoId);
﻿
﻿            
﻿
﻿                            if (pedidoComEndereco == null)
﻿
﻿                            {
﻿
﻿                                Console.WriteLine($"[LOG] ERRO CRÍTICO: Pedido com ID {primeiroPedidoId} não foi encontrado no banco de dados.");
﻿
﻿                            }
﻿
﻿                            else if (pedidoComEndereco.EnderecoEntrega == null)
﻿
﻿                            {
﻿
﻿                                Console.WriteLine($"[LOG] O Pedido #{pedidoComEndereco.Id} FOI encontrado, mas seu EnderecoEntrega é NULO. Pulando verificação de clima.");
﻿
﻿                            }
﻿
﻿                            else
﻿
﻿                            {
﻿
﻿                                Console.WriteLine($"[LOG] Pedido #{pedidoComEndereco.Id} e seu EnderecoEntrega foram encontrados. Prosseguindo para a lógica de clima...");
﻿
﻿                                var endereco = pedidoComEndereco.EnderecoEntrega;
﻿
﻿            
﻿
﻿                                // Se as coordenadas não existirem, tenta geocodificar agora
﻿
﻿                                if (endereco.Latitude == 0 || endereco.Longitude == 0)
﻿
﻿                                {
﻿
﻿                                    Console.WriteLine($"[LOG] Endereço sem coordenadas. Tentando geocodificar: {endereco.Logradouro}, {endereco.Numero}");
﻿
﻿                                    var coordenadas = await _geocodingService.ObterCoordenadasAsync(endereco);
﻿
﻿                                    if (coordenadas.HasValue)
﻿
﻿                                    {
﻿
﻿                                        Console.WriteLine($"[LOG] Geocodificação BEM-SUCEDIDA. Lat: {coordenadas.Value.Latitude}, Lon: {coordenadas.Value.Longitude}");
﻿
﻿                                        endereco.Latitude = coordenadas.Value.Latitude;
﻿
﻿                                        endereco.Longitude = coordenadas.Value.Longitude;
﻿
﻿                                        _context.Entry(endereco).State = EntityState.Modified;
﻿
﻿                                        await _context.SaveChangesAsync(); // Salva as novas coordenadas no endereço
﻿
﻿                                    }
﻿
﻿                                    else
﻿
﻿                                    {
﻿
﻿                                        Console.WriteLine("[LOG] Geocodificação FALHOU. Não foi possível obter coordenadas.");
﻿
﻿                                    }
﻿
﻿                                }
﻿
﻿            
﻿
﻿                                // Prossegue para a verificação do clima apenas se agora existirem coordenadas
﻿
﻿                                if (endereco.Latitude != 0 && endereco.Longitude != 0)
﻿
﻿                                {
﻿
﻿                                    Console.WriteLine($"[LOG] Coordenadas encontradas. Chamando o serviço de clima para Lat: {endereco.Latitude}, Lon: {endereco.Longitude}");
﻿
﻿                                    var weatherDto = await _weatherService.VerificarClimaAsync(endereco.Latitude, endereco.Longitude);
﻿
﻿                                    
﻿
﻿                                    if (weatherDto != null)
﻿
﻿                                    {
﻿
﻿                                        Console.WriteLine($"[LOG] Serviço de clima retornou DTO: Descricao={weatherDto.Descricao}, Temp={weatherDto.Temperatura}, Alerta={weatherDto.TipoAlerta ?? "N/A"}");
﻿
﻿                                        var novoAlerta = new AlertaClimatico
﻿
﻿                                        {
﻿
﻿                                            RotaId = rota.Id,
﻿
﻿                                            Descricao = weatherDto.Descricao,
﻿
﻿                                            Temperatura = weatherDto.Temperatura,
﻿
﻿                                            SensacaoTermica = weatherDto.SensacaoTermica,
﻿
﻿                                            Icone = weatherDto.Icone,
﻿
﻿                                            TipoAlerta = weatherDto.TipoAlerta,
﻿
﻿                                            Severidade = weatherDto.Severidade
﻿
﻿                                        };
﻿
﻿                                        _context.AlertasClimaticos.Add(novoAlerta);
﻿
﻿                                        await _context.SaveChangesAsync();
﻿
﻿                                        Console.WriteLine("[LOG] Novo AlertaClimatico salvo no banco de dados.");
﻿
﻿                                    }
﻿
﻿                                    else
﻿
﻿                                    {
﻿
﻿                                        Console.WriteLine("[LOG] Serviço de clima retornou NULL.");
﻿
﻿                                    }
﻿
﻿                
﻿
﻿                                }
﻿
﻿                                else
﻿
﻿                                {
﻿
﻿                                    Console.WriteLine("[LOG] Pulei a verificação de clima porque o endereço não tem coordenadas.");
﻿
﻿                                }
﻿
﻿                            }
﻿
﻿                        }
﻿
﻿                        else
﻿
﻿                        {
﻿
﻿                             Console.WriteLine("[LOG] A lista de PedidosIds está vazia. Pulando verificação de clima.");
﻿
﻿                        }
﻿
﻿                        return CreatedAtAction("GetRota", new { id = rota.Id }, rota);
﻿
﻿                    }
﻿
﻿        [HttpPut("{id}")]
﻿        public async Task<IActionResult> PutRota(int id, Rota rota)
﻿        {
﻿            if (id != rota.Id)
﻿            {
﻿                return BadRequest("O ID na URL não corresponde ao ID da rota fornecida.");
﻿            }
﻿
﻿            var existingRota = await _context.Rotas
﻿                                            .AsNoTracking()
﻿                                            .FirstOrDefaultAsync(r => r.Id == id);
﻿
﻿            if (existingRota == null)
﻿            {
﻿                return NotFound($"Rota com ID {id} não encontrada.");
﻿            }
﻿
﻿            // Validar se o status da rota permite alteração
﻿            if (existingRota.Status != StatusRota.Planejada)
﻿            {
﻿                return BadRequest("Não é possível alterar uma rota que não está 'Planejada'.");
﻿            }
﻿            
﻿            // Validar VeiculoId e MotoristaId
﻿            if (!await _context.Veiculos.AnyAsync(v => v.Id == rota.VeiculoId))
﻿            {
﻿                return BadRequest("Veículo associado não encontrado.");
﻿            }
﻿            if (!await _context.Motoristas.AnyAsync(m => m.Id == rota.MotoristaId))
﻿            {
﻿                return BadRequest("Motorista associado não encontrado.");
﻿            }
﻿
﻿            // Anexar a entidade e marcar como modificada
﻿            _context.Entry(rota).State = EntityState.Modified;
﻿            
﻿            try
﻿            {
﻿                await _context.SaveChangesAsync();
﻿            }
﻿            catch (DbUpdateConcurrencyException)
﻿            {
﻿                if (!RotaExists(id))
﻿                {
﻿                    return NotFound();
﻿                }
﻿                else
﻿                {
﻿                    throw;
﻿                }
﻿            }
﻿
﻿            return NoContent();
﻿        }
﻿
﻿        [HttpDelete("{id}")]
﻿        public async Task<IActionResult> DeleteRota(int id)
﻿        {
﻿            var rota = await _context.Rotas.FindAsync(id);
﻿            if (rota == null)
﻿            {
﻿                return NotFound();
﻿            }
﻿
﻿            if (rota.Status == StatusRota.EmAndamento)
﻿            {
﻿                return BadRequest("Não é possível excluir uma rota que está em andamento.");
﻿            }
﻿
﻿            _context.Rotas.Remove(rota);
﻿            await _context.SaveChangesAsync();
﻿
﻿            return NoContent();
﻿        }
﻿
﻿        [HttpPut("{rotaId}/pedidos/{pedidoId}/entregar")]
﻿        public async Task<IActionResult> MarcarPedidoComoEntregue(int rotaId, int pedidoId)
﻿        {
﻿            var rotaPedido = await _context.RotaPedidos
﻿                .FirstOrDefaultAsync(rp => rp.RotaId == rotaId && rp.PedidoId == pedidoId);
﻿
﻿            if (rotaPedido == null)
﻿            {
﻿                return NotFound("O vínculo entre esta rota e este pedido não foi encontrado.");
﻿            }
﻿
﻿            if (rotaPedido.StatusEntrega == "Entregue")
﻿            {
﻿                return BadRequest("Este pedido já está marcado como entregue nesta rota.");
﻿            }
﻿
﻿            rotaPedido.StatusEntrega = "Entregue";
﻿
﻿            var pedidoPrincipal = await _context.Pedidos.FindAsync(pedidoId);
﻿            if (pedidoPrincipal != null)
﻿            {
﻿                pedidoPrincipal.Status = StatusPedido.Entregue;
﻿                _context.Entry(pedidoPrincipal).State = EntityState.Modified;
﻿            }
﻿
﻿            await _context.SaveChangesAsync();
﻿
﻿            bool existemPendencias = await _context.RotaPedidos
﻿                .AnyAsync(rp => rp.RotaId == rotaId && rp.StatusEntrega != "Entregue");
﻿
﻿            if (!existemPendencias)
﻿            {
﻿                var rota = await _context.Rotas
﻿                    .Include(r => r.Veiculo)
﻿                    .FirstOrDefaultAsync(r => r.Id == rotaId);
﻿
﻿                if (rota != null)
﻿                {
﻿                    rota.Status = StatusRota.Concluida;
﻿                    _context.Entry(rota).State = EntityState.Modified;
﻿
﻿                    if (rota.Veiculo != null)
﻿                    {
﻿                        rota.Veiculo.Status = StatusVeiculo.Disponivel;
﻿                        _context.Entry(rota.Veiculo).State = EntityState.Modified;
﻿                    }
﻿
﻿                    await _context.SaveChangesAsync();
﻿                }
﻿            }
﻿            return NoContent();
﻿        }
﻿
﻿        private bool RotaExists(int id)
﻿        {
﻿            return _context.Rotas.Any(e => e.Id == id);
﻿        }
﻿    }
﻿
﻿    public class RotaRequest
﻿    {
﻿        public int VeiculoId { get; set; }
﻿        public int MotoristaId { get; set; }
﻿        public List<int> PedidosIds { get; set; } = new List<int>();
﻿    }
﻿}
﻿