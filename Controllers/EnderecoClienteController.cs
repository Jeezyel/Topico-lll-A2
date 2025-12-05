using A2.Data;
using A2.Models;
using A2.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace A2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        public async Task<ActionResult<IEnumerable<EnderecoCliente>>> GetEnderecos([FromQuery] int? clienteId)
        {
            var query = _context.EnderecosClientes.AsQueryable();

            if (clienteId.HasValue)
            {
                query = query.Where(e => e.ClienteId == clienteId.Value);
            }

            return await query.Include(e => e.Cliente).ToListAsync();
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
            if (id != endereco.Id)
            {
                return BadRequest("O ID na URL não corresponde ao ID do endereço fornecido.");
            }

            var existingEndereco = await _context.EnderecosClientes.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            if (existingEndereco == null)
            {
                return NotFound($"Endereço com ID {id} não encontrado.");
            }

            // Validação se o cliente associado ainda existe (se ClienteId foi alterado ou mesmo se manteve)
            if (!await _context.Clientes.AnyAsync(c => c.Id == endereco.ClienteId))
            {
                return BadRequest("Cliente associado não encontrado.");
            }

            // Detectar se houve mudança em campos que afetam a geocodificação
            bool shouldReGeocode = existingEndereco.CEP != endereco.CEP ||
                                   existingEndereco.Logradouro != endereco.Logradouro ||
                                   existingEndereco.Numero != endereco.Numero ||
                                   existingEndereco.Bairro != endereco.Bairro ||
                                   existingEndereco.Cidade != endereco.Cidade ||
                                   existingEndereco.UF != endereco.UF;

            // Atualiza as propriedades do endereço existente
            _context.Entry(endereco).State = EntityState.Modified;

            if (shouldReGeocode)
            {
                var coordenadas = await _geocodingService.ObterCoordenadasAsync(endereco);
                if (coordenadas.HasValue)
                {
                    endereco.Latitude = coordenadas.Value.Latitude;
                    endereco.Longitude = coordenadas.Value.Longitude;
                }
                else
                {
                    Console.WriteLine($"Aviso: Não foi possível re-geocodificar o endereço {endereco.Id}. Mantendo lat/lon zerados.");
                    // Opcional: manter os valores antigos ou zerar, dependendo da regra de negócio
                    endereco.Latitude = 0; 
                    endereco.Longitude = 0;
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EnderecoClienteExists(id))
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
        public async Task<IActionResult> DeleteEndereco(int id)
        {
            var endereco = await _context.EnderecosClientes.FindAsync(id);
            if (endereco == null)
            {
                return NotFound();
            }
            _context.EnderecosClientes.Remove(endereco);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool EnderecoClienteExists(int id)
        {
            return _context.EnderecosClientes.Any(e => e.Id == id);
        }
    }
}
