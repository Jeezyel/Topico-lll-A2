using A2.Data;
using A2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// using Microsoft.AspNetCore.Authorization; // Para controle de acesso por roles

namespace A2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize(Roles = "Administrador, Gerente")] // Apenas administradores e gerentes podem gerenciar veículos
    public class VeiculoController : ControllerBase
    {
        private readonly A2Context _context;

        public VeiculoController(A2Context context)
        {
            _context = context;
        }

        /// Retorna todos os veículos cadastrados.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Veiculo>>> GetVeiculos()
        {
            return await _context.Veiculos
                                .Include(v => v.Manutencoes)
                                .OrderBy(v => v.Placa)
                                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Veiculo>> GetVeiculo(int id)
        {
            var veiculo = await _context.Veiculos
                                        .Include(v => v.Manutencoes)
                                        .FirstOrDefaultAsync(v => v.Id == id);

            if (veiculo == null)
            {
                return NotFound();
            }

            return veiculo;
        }

        [HttpGet("DisponiveisParaRota")]
        public async Task<ActionResult<IEnumerable<Veiculo>>> GetVeiculosDisponiveisParaRota()
        {
            return await _context.Veiculos
                                .Where(v => v.Status == StatusVeiculo.Disponivel && v.DataProximaManutencao >= DateTime.Today)
                                .OrderBy(v => v.Placa)
                                .ToListAsync();
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Veiculo>> PostVeiculo(Veiculo veiculo)
        {
            if (await _context.Veiculos.AnyAsync(v => v.Placa == veiculo.Placa))
            {
                return BadRequest("Veículo com esta placa já cadastrado.");
            }

            veiculo.Status = StatusVeiculo.Disponivel;
            if (veiculo.DataUltimaManutencao == DateTime.MinValue) veiculo.DataUltimaManutencao = DateTime.Today;
            if (veiculo.DataProximaManutencao == DateTime.MinValue) veiculo.DataProximaManutencao = DateTime.Today.AddYears(1); // Exemplo padrão

            _context.Veiculos.Add(veiculo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVeiculo", new { id = veiculo.Id }, veiculo);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutVeiculo(int id, Veiculo veiculo)
        {
            if (id != veiculo.Id)
            {
                return BadRequest();
            }

            if (await _context.Veiculos.AnyAsync(v => v.Placa == veiculo.Placa && v.Id != veiculo.Id))
            {
                return BadRequest("Placa já cadastrada para outro veículo.");
            }

            var existingVeiculo = await _context.Veiculos.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
            if (existingVeiculo == null)
            {
                return NotFound();
            }

            _context.Entry(veiculo).State = EntityState.Modified;


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VeiculoExists(id))
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
        [Authorize]
        public async Task<IActionResult> DeleteVeiculo(int id)
        {
            var veiculo = await _context.Veiculos.FindAsync(id);
            if (veiculo == null)
            {
                return NotFound();
            }

            _context.Veiculos.Remove(veiculo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VeiculoExists(int id)
        {
            return _context.Veiculos.Any(e => e.Id == id);
        }
    }
}