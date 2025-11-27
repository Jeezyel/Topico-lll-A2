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
    public class LogIntegracaoController : ControllerBase
    {
        private readonly A2Context _context;

        public LogIntegracaoController(A2Context context)
        {
            _context = context;
        }

        // GET: api/LogIntegracao
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LogIntegracao>>> GetLogsIntegracao()
        {
            return await _context.LogsIntegracao.ToListAsync();
        }

        // GET: api/LogIntegracao/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<LogIntegracao>> GetLogIntegracao(int id)
        {
            var logIntegracao = await _context.LogsIntegracao.FindAsync(id);

            if (logIntegracao == null)
                return NotFound();

            return logIntegracao;
        }

        // POST: api/LogIntegracao
        [HttpPost]
        public async Task<ActionResult<LogIntegracao>> PostLogIntegracao(LogIntegracao logIntegracao)
        {
            _context.LogsIntegracao.Add(logIntegracao);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetLogIntegracao", new { id = logIntegracao.Id }, logIntegracao);
        }

        // PUT: api/LogIntegracao/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLogIntegracao(int id, LogIntegracao logIntegracao)
        {
            if (id != logIntegracao.Id)
                return BadRequest();

            _context.Entry(logIntegracao).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.LogsIntegracao.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/LogIntegracao/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLogIntegracao(int id)
        {
            var logIntegracao = await _context.LogsIntegracao.FindAsync(id);
            if (logIntegracao == null)
                return NotFound();

            _context.LogsIntegracao.Remove(logIntegracao);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
