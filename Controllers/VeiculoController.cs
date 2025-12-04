using A2.Data;
﻿using A2.DTOs; // Usar o novo DTO
﻿using A2.Models;
﻿using Microsoft.AspNetCore.Authorization;
﻿using Microsoft.AspNetCore.Mvc;
﻿using Microsoft.EntityFrameworkCore;
﻿
﻿namespace A2.Controllers
﻿{
﻿    [ApiController]
﻿    [Route("api/[controller]")]
﻿    [Authorize] 
﻿    public class VeiculoController : ControllerBase
﻿    {
﻿        private readonly A2Context _context;
﻿
﻿        public VeiculoController(A2Context context)
﻿        {
﻿            _context = context;
﻿        }
﻿
﻿        [HttpGet]
﻿        public async Task<ActionResult<IEnumerable<Veiculo>>> GetVeiculos()
﻿        {
﻿            return await _context.Veiculos
﻿                                .AsNoTracking()
﻿                                .Include(v => v.Manutencoes)
﻿                                .OrderBy(v => v.Placa)
﻿                                .ToListAsync();
﻿        }
﻿
﻿        [HttpGet("{id}")]
﻿        public async Task<ActionResult<Veiculo>> GetVeiculo(int id)
﻿        {
﻿            var veiculo = await _context.Veiculos
﻿                                        .AsNoTracking()
﻿                                        .Include(v => v.Manutencoes)
﻿                                        .FirstOrDefaultAsync(v => v.Id == id);
﻿            if (veiculo == null) return NotFound();
﻿            return veiculo;
﻿        }
﻿
﻿        [HttpGet("DisponiveisParaRota")]
﻿        public async Task<ActionResult<IEnumerable<Veiculo>>> GetVeiculosDisponiveisParaRota()
﻿        {
﻿            return await _context.Veiculos
﻿                                .AsNoTracking()
﻿                                .Where(v => v.Status == StatusVeiculo.Disponivel && v.DataProximaManutencao >= DateTime.Today)
﻿                                .OrderBy(v => v.Placa)
﻿                                .ToListAsync();
﻿        }
﻿
﻿        [HttpPost]
﻿        public async Task<ActionResult<Veiculo>> PostVeiculo(VeiculoCreateUpdateDto veiculoDto)
﻿        {
﻿            if (await _context.Veiculos.AnyAsync(v => v.Placa == veiculoDto.Placa))
﻿            {
﻿                return BadRequest("Veículo com esta placa já cadastrado.");
﻿            }
﻿
﻿            var veiculo = new Veiculo
﻿            {
﻿                Placa = veiculoDto.Placa,
﻿                Marca = veiculoDto.Marca,
﻿                Modelo = veiculoDto.Modelo,
﻿                AnoFabricacao = veiculoDto.AnoFabricacao,
﻿                CapacidadeCarga = veiculoDto.CapacidadeCarga,
﻿                CapacidadeVolume = veiculoDto.CapacidadeVolume,
﻿                Status = StatusVeiculo.Disponivel, // Status inicial é sempre Disponível
﻿                DataUltimaManutencao = veiculoDto.DataUltimaManutencao ?? DateTime.Today,
﻿                DataProximaManutencao = veiculoDto.DataProximaManutencao ?? DateTime.Today.AddYears(1)
﻿            };
﻿
﻿            _context.Veiculos.Add(veiculo);
﻿            await _context.SaveChangesAsync();
﻿
﻿            return CreatedAtAction("GetVeiculo", new { id = veiculo.Id }, veiculo);
﻿        }
﻿
﻿        [HttpPut("{id}")]
﻿        public async Task<IActionResult> PutVeiculo(int id, VeiculoCreateUpdateDto veiculoDto)
﻿        {
﻿            if (await _context.Veiculos.AnyAsync(v => v.Placa == veiculoDto.Placa && v.Id != id))
﻿            {
﻿                return BadRequest("Placa já cadastrada para outro veículo.");
﻿            }
﻿
﻿            var existingVeiculo = await _context.Veiculos.FindAsync(id);
﻿            if (existingVeiculo == null)
﻿            {
﻿                return NotFound();
﻿            }
﻿
﻿            // Mapeia os dados do DTO para a entidade existente (padrão "read-then-update")
﻿            existingVeiculo.Placa = veiculoDto.Placa;
﻿            existingVeiculo.Marca = veiculoDto.Marca;
﻿            existingVeiculo.Modelo = veiculoDto.Modelo;
﻿            existingVeiculo.AnoFabricacao = veiculoDto.AnoFabricacao;
﻿            existingVeiculo.CapacidadeCarga = veiculoDto.CapacidadeCarga;
﻿            existingVeiculo.CapacidadeVolume = veiculoDto.CapacidadeVolume;
﻿            existingVeiculo.Status = (StatusVeiculo)veiculoDto.Status;
﻿            
﻿            if (veiculoDto.DataUltimaManutencao.HasValue)
﻿                existingVeiculo.DataUltimaManutencao = veiculoDto.DataUltimaManutencao.Value;
﻿
﻿            if (veiculoDto.DataProximaManutencao.HasValue)
﻿                existingVeiculo.DataProximaManutencao = veiculoDto.DataProximaManutencao.Value;
﻿
﻿            try
﻿            {
﻿                await _context.SaveChangesAsync();
﻿            }
﻿            catch (DbUpdateConcurrencyException)
﻿            {
﻿                if (!VeiculoExists(id)) return NotFound();
﻿                else throw;
﻿            }
﻿
﻿            return NoContent();
﻿        }
﻿
﻿        [HttpDelete("{id}")]
﻿        public async Task<IActionResult> DeleteVeiculo(int id)
﻿        {
﻿            var veiculo = await _context.Veiculos.FindAsync(id);
﻿            if (veiculo == null) return NotFound();
﻿
﻿            _context.Veiculos.Remove(veiculo);
﻿            await _context.SaveChangesAsync();
﻿
﻿            return NoContent();
﻿        }
﻿
﻿        private bool VeiculoExists(int id)
﻿        {
﻿            return _context.Veiculos.Any(e => e.Id == id);
﻿        }
﻿    }
﻿}
﻿