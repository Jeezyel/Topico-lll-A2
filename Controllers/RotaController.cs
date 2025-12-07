using A2.Data;
﻿using A2.Models;
﻿using A2.Service;
﻿using Microsoft.AspNetCore.Authorization;
﻿using Microsoft.AspNetCore.Mvc;
﻿using Microsoft.EntityFrameworkCore;
﻿using System;
﻿using System.Collections.Generic;
﻿using System.Linq;
﻿using System.Security.Claims;
﻿using System.Threading.Tasks;
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
﻿        private readonly IGeocodingService _geocodingService;
﻿        private readonly ILogger<RotaController> _logger;
        private WeatherForces weatherForcesdb = new WeatherForces();


        public RotaController(A2Context context, IWeatherService weatherService, IGeocodingService geocodingService, ILogger<RotaController> logger)
﻿        {
﻿            _context = context;
﻿            _weatherService = weatherService;
﻿            _geocodingService = geocodingService;
﻿            _logger = logger;
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
﻿            // Se a rota não tiver alertas climáticos, tenta buscar um agora.
﻿            if (!rota.AlertasClimaticos.Any())
﻿            {
﻿                Console.WriteLine($"[LOG] Rota {id} não possui alerta de clima. Tentando obter agora.");
﻿                await VerificarEAdicionarAlertaClimaticoAsync(rota);
﻿            }
﻿
﻿            return rota;
﻿        }
﻿
﻿                [HttpPost]
﻿                public async Task<ActionResult<Rota>> PostRota(RotaRequest request)
﻿                {
﻿                    var veiculo = await _context.Veiculos.FindAsync(request.VeiculoId);
﻿                    if (veiculo == null) return BadRequest("Veículo inválido.");
﻿        
﻿                    if (veiculo.DataProximaManutencao < DateTime.Today)
﻿                        return BadRequest("Veículo está com a manutenção atrasada.");
﻿        
﻿                    if (veiculo.Status != StatusVeiculo.Disponivel)
﻿                        return BadRequest("Veículo não está disponível.");
﻿        
﻿                    var motorista = await _context.Motoristas.FindAsync(request.MotoristaId);
﻿                    if (motorista == null) return BadRequest("Motorista inválido.");
﻿        
﻿                    var pedidos = await _context.Pedidos
﻿                        .Include(p => p.Cliente)
﻿                        .Include(p => p.EnderecoEntrega)
﻿                        .Where(p => request.PedidosIds.Contains(p.Id) && p.Status == StatusPedido.Pendente)
﻿                        .ToListAsync();
﻿        
﻿                    if (pedidos.Count != request.PedidosIds.Count)
﻿                        return BadRequest("Alguns pedidos não foram encontrados ou não estão pendentes.");
﻿        
﻿                    // Validação da Janela de Horário
﻿                    var dataRota = DateTime.Now; // A rota é criada para o dia de hoje.
﻿                    foreach (var pedido in pedidos)
﻿                    {
﻿                        if (pedido.EnderecoEntregaId == 0 || pedido.EnderecoEntrega == null)
﻿                        {
﻿                            return BadRequest($"O pedido #{pedido.Id} não possui um endereço de entrega válido.");
﻿                        }
﻿        
﻿                                                /*
﻿                                                var janelasDoEndereco = await _context.JanelasHorarias
﻿                                                    .Where(j => j.EnderecoClienteId == pedido.EnderecoEntrega.Id)
﻿                                                    .ToListAsync();
﻿                                
﻿                                                if (!IsDentroDaJanela(dataRota, janelasDoEndereco))
﻿                                                {
﻿                                                    return BadRequest($"O pedido #{pedido.Id} para o cliente '{pedido.Cliente.NomeEmpresa}' não pode ser entregue na data de hoje ({dataRota:dd/MM/yyyy}), pois está fora da janela de horário do cliente.");
﻿                                                }
﻿                                                */﻿                    }
﻿        
﻿                    decimal pesoTotal = pedidos.Sum(p => p.PesoTotalKg);
﻿                    decimal volumeTotal = pedidos.Sum(p => p.VolumeTotalM3);
﻿        
﻿                    if (pesoTotal > veiculo.CapacidadeCarga)
﻿                        return BadRequest($"Peso excedido! Carga: {pesoTotal}kg. Veículo suporta: {veiculo.CapacidadeCarga}kg.");
﻿        
﻿                    if (volumeTotal > veiculo.CapacidadeVolume)
﻿                        return BadRequest($"Volume excedido! Carga: {volumeTotal}m3. Veículo suporta: {veiculo.CapacidadeVolume}m3.");
﻿        
﻿                    var rota = new Rota
﻿                    {
﻿                        VeiculoId = request.VeiculoId,
﻿                        MotoristaId = request.MotoristaId,
﻿                        Status = StatusRota.Planejada,
﻿                        DataRota = dataRota
﻿                    };
﻿        
﻿                    _context.Rotas.Add(rota);
﻿                    await _context.SaveChangesAsync(); 
﻿        
﻿                    int ordemContador = 1;
﻿        
﻿                    foreach (var p in pedidos)
﻿                    {
﻿                        var rotaPedido = new RotaPedido
﻿                        {
﻿                            RotaId = rota.Id,
﻿                            PedidoId = p.Id,
﻿                            OrdemEntrega = ordemContador++,
﻿                            StatusEntrega = "Pendente"
﻿                        };
﻿                        _context.RotaPedidos.Add(rotaPedido);
                        rota.RotaPedidos.Add(rotaPedido);
﻿        
﻿                        p.Status = StatusPedido.EmRota;
﻿                        _context.Entry(p).State = EntityState.Modified;
﻿                    }
﻿                    veiculo.Status = StatusVeiculo.EmRota;
﻿                    _context.Entry(veiculo).State = EntityState.Modified;
﻿        
﻿                    await _context.SaveChangesAsync();
﻿        
﻿                    // --- LÓGICA DE GEOLOCALIZAÇÃO E CLIMA REATORADA ---
﻿                    await VerificarEAdicionarAlertaClimaticoAsync(rota);
﻿        
﻿                    return CreatedAtAction("GetRota", new { id = rota.Id }, rota);
﻿                }
﻿        
﻿                [HttpPut("{id}")]
﻿                public async Task<IActionResult> PutRota(int id, Rota rota)
﻿                {
﻿                    if (id != rota.Id)
﻿                    {
﻿                        return BadRequest("O ID na URL não corresponde ao ID da rota fornecida.");
﻿                    }
﻿        
﻿                    var existingRota = await _context.Rotas
﻿                                                    .AsNoTracking()
﻿                                                    .FirstOrDefaultAsync(r => r.Id == id);
﻿        
﻿                    if (existingRota == null)
﻿                    {
﻿                        return NotFound($"Rota com ID {id} não encontrada.");
﻿                    }
﻿        
﻿                    // Validar se o status da rota permite alteração
﻿                    if (existingRota.Status != StatusRota.Planejada)
﻿                    {
﻿                        return BadRequest("Não é possível alterar uma rota que não está 'Planejada'.");
﻿                    }
﻿                    
﻿                    // Validar VeiculoId e MotoristaId
﻿                    if (!await _context.Veiculos.AnyAsync(v => v.Id == rota.VeiculoId))
﻿                    {
﻿                        return BadRequest("Veículo associado não encontrado.");
﻿                    }
﻿                    if (!await _context.Motoristas.AnyAsync(m => m.Id == rota.MotoristaId))
﻿                    {
﻿                        return BadRequest("Motorista associado não encontrado.");
﻿                    }
﻿        
﻿                    // Anexar a entidade e marcar como modificada
﻿                    _context.Entry(rota).State = EntityState.Modified;
﻿                    
﻿                    try
﻿                    {
﻿                        await _context.SaveChangesAsync();
﻿                    }
﻿                    catch (DbUpdateConcurrencyException)
﻿                    {
﻿                        if (!RotaExists(id))
﻿                        {
﻿                            return NotFound();
﻿                        }
﻿                        else
﻿                        {
﻿                            throw;
﻿                        }
﻿                    }
﻿        
﻿                    return NoContent();
﻿                }
﻿        
﻿                [HttpDelete("{id}")]
﻿                public async Task<IActionResult> DeleteRota(int id)
﻿                {
﻿                    var rota = await _context.Rotas.FindAsync(id);
﻿                    if (rota == null)
﻿                    {
﻿                        return NotFound();
﻿                    }
﻿        
﻿                    if (rota.Status == StatusRota.EmAndamento)
﻿                    {
﻿                        return BadRequest("Não é possível excluir uma rota que está em andamento.");
﻿                    }
﻿        
﻿                    _context.Rotas.Remove(rota);
﻿                    await _context.SaveChangesAsync();
﻿        
﻿                    return NoContent();
﻿                }
﻿        
﻿                [HttpPut("{rotaId}/pedidos/{pedidoId}/entregar")]
﻿                public async Task<IActionResult> MarcarPedidoComoEntregue(int rotaId, int pedidoId)
﻿                {
﻿                    var rotaPedido = await _context.RotaPedidos
﻿                        .FirstOrDefaultAsync(rp => rp.RotaId == rotaId && rp.PedidoId == pedidoId);
﻿        
﻿                    if (rotaPedido == null)
﻿                    {
﻿                        return NotFound("O vínculo entre esta rota e este pedido não foi encontrado.");
﻿                    }
﻿        
﻿                    if (rotaPedido.StatusEntrega == "Entregue")
﻿                    {
﻿                        return BadRequest("Este pedido já está marcado como entregue nesta rota.");
﻿                    }
﻿        
﻿                    rotaPedido.StatusEntrega = "Entregue";
﻿        
﻿                    var pedidoPrincipal = await _context.Pedidos.FindAsync(pedidoId);
﻿                    if (pedidoPrincipal != null)
﻿                    {
﻿                        pedidoPrincipal.Status = StatusPedido.Entregue;
﻿                        _context.Entry(pedidoPrincipal).State = EntityState.Modified;
﻿                    }
﻿        
﻿                    await _context.SaveChangesAsync();
﻿        
﻿                    bool existemPendencias = await _context.RotaPedidos
﻿                        .AnyAsync(rp => rp.RotaId == rotaId && rp.StatusEntrega != "Entregue");
﻿        
﻿                    if (!existemPendencias)
﻿                    {
﻿                        var rota = await _context.Rotas
﻿                            .Include(r => r.Veiculo)
﻿                            .FirstOrDefaultAsync(r => r.Id == rotaId);
﻿        
﻿                        if (rota != null)
﻿                        {
﻿                            rota.Status = StatusRota.Concluida;
﻿                            _context.Entry(rota).State = EntityState.Modified;
﻿        
﻿                            if (rota.Veiculo != null)
﻿                            {
﻿                                rota.Veiculo.Status = StatusVeiculo.Disponivel;
﻿                                _context.Entry(rota.Veiculo).State = EntityState.Modified;
﻿                            }
﻿        
﻿                            await _context.SaveChangesAsync();
﻿                        }
﻿                    }
﻿                    return NoContent();
﻿                }
﻿        
﻿                private bool RotaExists(int id)
﻿                {
﻿                    return _context.Rotas.Any(e => e.Id == id);
﻿                }
﻿        
﻿                                                private bool IsDentroDaJanela(DateTime data, ICollection<JanelaHorario> janelas)
﻿        
﻿                                                {
﻿        
﻿                                                    // Validação da janela de horário foi temporariamente desativada para simplificar o fluxo de desenvolvimento.
﻿        
﻿                                                    return true;
﻿        
﻿                                                }﻿        private async Task VerificarEAdicionarAlertaClimaticoAsync(Rota rota)
﻿        {
﻿            var primeiroRotaPedido = rota.RotaPedidos.OrderBy(rp => rp.OrdemEntrega).FirstOrDefault();
﻿            if (primeiroRotaPedido == null)
﻿            {
﻿                Console.WriteLine($"[LOG] Rota {rota.Id} não tem pedidos associados. Pulando verificação de clima.");
﻿                return;
﻿            }
﻿
﻿            var pedidoComEndereco = await _context.Pedidos
﻿                .Include(p => p.EnderecoEntrega)
﻿                .FirstOrDefaultAsync(p => p.Id == primeiroRotaPedido.PedidoId);
﻿
﻿            if (pedidoComEndereco == null)
﻿            {
﻿                Console.WriteLine($"[LOG] ERRO CRÍTICO: Pedido com ID {primeiroRotaPedido.PedidoId} não foi encontrado.");
﻿                return;
﻿            }
﻿
﻿            if (pedidoComEndereco.EnderecoEntrega == null)
﻿            {
﻿                Console.WriteLine($"[LOG] Pedido #{pedidoComEndereco.Id} não tem EnderecoEntrega. Pulando verificação de clima.");
﻿                return;
﻿            }
﻿
﻿            Console.WriteLine($"[LOG] Verificando clima para o endereço do pedido #{pedidoComEndereco.Id} na rota #{rota.Id}.");
﻿
﻿            var endereco = pedidoComEndereco.EnderecoEntrega;
﻿
﻿            if (endereco.Latitude == 0 || endereco.Longitude == 0)
﻿            {
﻿                Console.WriteLine($"[LOG] Endereço sem coordenadas. Tentando geocodificar: {endereco.Logradouro}, {endereco.Numero}");
﻿                var coordenadas = await _geocodingService.ObterCoordenadasAsync(endereco);
﻿                if (coordenadas.HasValue)
﻿                {
﻿                    Console.WriteLine($"[LOG] Geocodificação BEM-SUCEDIDA. Lat: {coordenadas.Value.Latitude}, Lon: {coordenadas.Value.Longitude}");
﻿                    endereco.Latitude = coordenadas.Value.Latitude;
﻿                    endereco.Longitude = coordenadas.Value.Longitude;
﻿                    _context.Entry(endereco).State = EntityState.Modified;
﻿                    await _context.SaveChangesAsync();
﻿                }
﻿                else
﻿                {
﻿                    Console.WriteLine("[LOG] Geocodificação FALHOU.");
﻿                }
﻿            }
﻿
﻿            if (endereco.Latitude != 0 && endereco.Longitude != 0)
﻿            {
﻿                Console.WriteLine($"[LOG] Coordenadas encontradas. Chamando o serviço de clima para Lat: {endereco.Latitude}, Lon: {endereco.Longitude}");
﻿                var weatherDto = await _weatherService.VerificarClimaAsync(endereco.Latitude, endereco.Longitude);

                // Atualiza o objeto WeatherForces no banco de dados

                try
                {

                    weatherForcesdb.Descricao = weatherDto.Descricao;
                    weatherForcesdb.Temperatura = weatherDto.Temperatura;
                    weatherForcesdb.SensacaoTermica = weatherDto.SensacaoTermica;
                    weatherForcesdb.Icone = weatherDto.Icone;
                    weatherForcesdb.TipoAlerta = weatherDto.TipoAlerta;
                    weatherForcesdb.Severidade = weatherDto.Severidade;
                    weatherForcesdb.latitude = endereco.Latitude;
                    weatherForcesdb.longitude = endereco.Longitude;

                    _context.Entry(weatherForcesdb).State = EntityState.Modified;
                }catch (Exception ex)
                {
                    Console.WriteLine($"[LOG] Erro ao atualizar WeatherForces: {ex.Message}");
                }



                if (weatherDto != null)
﻿                {
﻿                    Console.WriteLine($"[LOG] Serviço de clima retornou DTO: Descricao={weatherDto.Descricao}, Temp={weatherDto.Temperatura}, Alerta={weatherDto.TipoAlerta ?? "N/A"}");
﻿
﻿                    if (!rota.AlertasClimaticos.Any(a => a.Descricao == weatherDto.Descricao && a.Temperatura == weatherDto.Temperatura))
﻿                    {
﻿                        var novoAlerta = new AlertaClimatico
﻿                        {
﻿                            RotaId = rota.Id,
﻿                            Descricao = weatherDto.Descricao,
﻿                            Temperatura = weatherDto.Temperatura,
﻿                            SensacaoTermica = weatherDto.SensacaoTermica,
﻿                            Icone = weatherDto.Icone,
﻿                            TipoAlerta = weatherDto.TipoAlerta,
﻿                            Severidade = weatherDto.Severidade
﻿                        };
﻿
﻿                        _context.AlertasClimaticos.Add(novoAlerta);
﻿                        await _context.SaveChangesAsync();
﻿
﻿                        rota.AlertasClimaticos.Add(novoAlerta);
﻿                        Console.WriteLine("[LOG] Novo AlertaClimatico salvo e adicionado à rota.");
﻿                    }
﻿                    else
﻿                    {
﻿                        Console.WriteLine("[LOG] Alerta de clima idêntico já existe para esta rota. Nenhum novo alerta foi adicionado.");
﻿                    }
﻿                }
﻿                else
﻿                {
﻿                    Console.WriteLine("[LOG] Serviço de clima retornou NULL.");
﻿                }
﻿            }
﻿            else
﻿            {
﻿                Console.WriteLine("[LOG] Pulei a verificação de clima porque o endereço não tem coordenadas.");
﻿            }
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