using A2.Data;
using A2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace A2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfiguracaoSistemaController : ControllerBase
    {
        private readonly A2Context _context;

        public ConfiguracaoSistemaController(A2Context context)
        {
            _context = context;
        }

        // GET: api/ConfiguracaoSistema
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConfiguracaoSistema>>> GetConfiguracoesSistema()
        {
            return await _context.ConfiguracoesSistema.ToListAsync();
        }

        // GET: api/ConfiguracaoSistema/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ConfiguracaoSistema>> GetConfiguracaoSistema(int id)
        {
            var configuracaoSistema = await _context.ConfiguracoesSistema.FindAsync(id);

            if (configuracaoSistema == null)
                return NotFound();

            return configuracaoSistema;
        }

        // POST: api/ConfiguracaoSistema
        [HttpPost]
        public async Task<ActionResult<ConfiguracaoSistema>> PostConfiguracaoSistema(ConfiguracaoSistema configuracaoSistema)
        {
            _context.ConfiguracoesSistema.Add(configuracaoSistema);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetConfiguracaoSistema", new { id = configuracaoSistema.Id }, configuracaoSistema);
        }

        // PUT: api/ConfiguracaoSistema/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutConfiguracaoSistema(int id, ConfiguracaoSistema configuracaoSistema)
        {
            if (id != configuracaoSistema.Id)
                return BadRequest();

            _context.Entry(configuracaoSistema).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ConfiguracoesSistema.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/ConfiguracaoSistema/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConfiguracaoSistema(int id)
        {
            var configuracaoSistema = await _context.ConfiguracoesSistema.FindAsync(id);
            if (configuracaoSistema == null)
                return NotFound();

            _context.ConfiguracoesSistema.Remove(configuracaoSistema);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
