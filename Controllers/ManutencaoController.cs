using A2.Models;
using A2.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// using Microsoft.AspNetCore.Authorization; // Para controle de acesso por roles

namespace A2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize(Roles = "Administrador, Gerente")] // Apenas administradores e gerentes podem gerenciar manutenções
    public class ManutencaoController : ControllerBase
    {
        private readonly A2Context _context;

        public ManutencaoController(A2Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Manutencao>>> GetManutencoes()
        {
            return await _context.Manutencoes.Include(m => m.Veiculo).OrderByDescending(m => m.DataInicio).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Manutencao>> GetManutencao(int id)
        {
            var manutencao = await _context.Manutencoes
                                           .Include(m => m.Veiculo)
                                           .FirstOrDefaultAsync(m => m.Id == id);

            if (manutencao == null)
            {
                return NotFound();
            }

            return manutencao;
        }

        [HttpGet("Veiculo/{veiculoId}")]
        public async Task<ActionResult<IEnumerable<Manutencao>>> GetManutencoesByVeiculo(int veiculoId)
        {
            return await _context.Manutencoes
                                .Where(m => m.VeiculoId == veiculoId)
                                .OrderByDescending(m => m.DataInicio)
                                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Manutencao>> PostManutencao(Manutencao manutencao)
        {
            var veiculo = await _context.Veiculos.FindAsync(manutencao.VeiculoId);
            if (veiculo == null)
            {
                return NotFound("Veículo não encontrado.");
            }

            _context.Manutencoes.Add(manutencao);

            if (manutencao.DataFim == null)
            {
                veiculo.Status = StatusVeiculo.EmManutencao;
            }
            else
            {
                veiculo.Status = StatusVeiculo.Disponivel;
                if (manutencao.Tipo == TipoManutencao.Preventiva)
                {
                    veiculo.DataProximaManutencao = manutencao.DataFim.Value.AddMonths(6);
                }
                veiculo.DataUltimaManutencao = manutencao.DataFim.Value;
            }

            _context.Entry(veiculo).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetManutencao", new { id = manutencao.Id }, manutencao);
        }

        [HttpPut("{id}/Finalizar")]
        public async Task<IActionResult> FinalizarManutencao(int id, Manutencao manutencaoAtualizada)
        {
            var manutencao = await _context.Manutencoes.Include(m => m.Veiculo).FirstOrDefaultAsync(m => m.Id == id);
            if (manutencao == null)
            {
                return NotFound("Manutenção não encontrada.");
            }

            if (manutencao.DataFim != null)
            {
                return BadRequest("Esta manutenção já foi finalizada anteriormente.");
            }

            manutencao.DataFim = manutencaoAtualizada.DataFim ?? DateTime.Now;
            manutencao.Custo = manutencaoAtualizada.Custo;
            manutencao.OficinaOuResponsavel = manutencaoAtualizada.OficinaOuResponsavel;

            _context.Entry(manutencao).State = EntityState.Modified;

            if (manutencao.Veiculo != null)
            {
                manutencao.Veiculo.Status = StatusVeiculo.Disponivel;
                manutencao.Veiculo.DataUltimaManutencao = manutencao.DataFim.Value;

                if (manutencao.Tipo == TipoManutencao.Preventiva)
                {
                    manutencao.Veiculo.DataProximaManutencao = manutencao.DataFim.Value.AddMonths(6);
                }
                _context.Entry(manutencao.Veiculo).State = EntityState.Modified;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ManutencaoExists(id))
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
        public async Task<IActionResult> DeleteManutencao(int id)
        {
            var manutencao = await _context.Manutencoes.FindAsync(id);
            if (manutencao == null)
            {
                return NotFound();
            }

            _context.Manutencoes.Remove(manutencao);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ManutencaoExists(int id)
        {
            return _context.Manutencoes.Any(e => e.Id == id);
        }
    }
}