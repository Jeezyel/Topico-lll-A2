using A2.Data;
using A2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace A2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AlertaClimaticoController : ControllerBase
    {
        private readonly A2Context _context;

        public AlertaClimaticoController(A2Context context)
        {
            _context = context;
        }

        // GET: api/AlertaClimatico
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AlertaClimatico>>> GetAlertasClimaticos()
        {
            // Inclui a Rota na listagem
            return await _context.AlertasClimaticos.Include(a => a.Rota).ToListAsync();
        }

        // GET: api/AlertaClimatico/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AlertaClimatico>> GetAlertaClimatico(int id)
        {
            var alertaClimatico = await _context.AlertasClimaticos
                                                 .Include(a => a.Rota)
                                                 .FirstOrDefaultAsync(a => a.Id == id);

            if (alertaClimatico == null)
                return NotFound();

            return alertaClimatico;
        }

        // POST: api/AlertaClimatico
        [HttpPost]
        public async Task<ActionResult<AlertaClimatico>> PostAlertaClimatico(AlertaClimatico alertaClimatico)
        {
            _context.AlertasClimaticos.Add(alertaClimatico);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetAlertaClimatico", new { id = alertaClimatico.Id }, alertaClimatico);
        }

        // PUT: api/AlertaClimatico/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAlertaClimatico(int id, AlertaClimatico alertaClimatico)
        {
            if (id != alertaClimatico.Id)
                return BadRequest();

            _context.Entry(alertaClimatico).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.AlertasClimaticos.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/AlertaClimatico/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlertaClimatico(int id)
        {
            var alertaClimatico = await _context.AlertasClimaticos.FindAsync(id);
            if (alertaClimatico == null)
                return NotFound();

            _context.AlertasClimaticos.Remove(alertaClimatico);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
