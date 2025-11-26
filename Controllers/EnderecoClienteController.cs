using A2.Data;
using A2.Models;
using A2.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace A2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnderecoClientesController : ControllerBase
    {
        private readonly A2Context _context;
        private readonly IGeocodingService _geocodingService;

        public EnderecoClientesController(A2Context context, IGeocodingService geocodingService)
        {
            _context = context;
            _geocodingService = geocodingService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EnderecoCliente>>> GetEnderecos()
        {
            return await _context.EnderecosClientes.Include(e => e.Cliente).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EnderecoCliente>> GetEndereco(int id)
        {
            var endereco = await _context.EnderecosClientes
                                         .Include(e => e.Cliente)
                                         .FirstOrDefaultAsync(e => e.Id == id);
            if (endereco == null) return NotFound();
            return endereco;
        }

        [HttpPost]
        public async Task<ActionResult<EnderecoCliente>> PostEndereco(EnderecoCliente endereco)
        {
            // Validação básica se o cliente existe
            if (!await _context.Clientes.AnyAsync(c => c.Id == endereco.ClienteId))
            {
                return BadRequest("Cliente não encontrado.");
            }

            // --- INTEGRAÇÃO COM NOMINATIM ---
            // Chama o serviço para tentar obter as coordenadas
            var coordenadas = await _geocodingService.ObterCoordenadasAsync(endereco);

            if (coordenadas.HasValue)
            {
                // Se deu certo, preenche os campos
                endereco.Latitude = coordenadas.Value.Latitude;
                endereco.Longitude = coordenadas.Value.Longitude;
            }
            else
            {
                Console.WriteLine("Aviso: Não foi possível geocodificar o endereço. Salvando com lat/lon zerados.");
            }

            _context.EnderecosClientes.Add(endereco);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEndereco", new { id = endereco.Id }, endereco);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutEndereco(int id, EnderecoCliente endereco)
        {
            if (id != endereco.Id) return BadRequest();
            _context.Entry(endereco).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.EnderecosClientes.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEndereco(int id)
        {
            var endereco = await _context.EnderecosClientes.FindAsync(id);
            if (endereco == null) return NotFound();
            _context.EnderecosClientes.Remove(endereco);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
