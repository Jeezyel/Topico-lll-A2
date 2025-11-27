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
    public class ClienteController : ControllerBase
    {
        private readonly A2Context _context;

        public ClienteController(A2Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            // Inclui os endereços na listagem
            return await _context.Clientes.Include(c => c.Enderecos).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            var cliente = await _context.Clientes
                                        .Include(c => c.Enderecos)
                                        .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null) return NotFound();
            return cliente;
        }

        [HttpPost]
        public async Task<ActionResult<Cliente>> PostCliente(Cliente cliente)
        {
            if (await _context.Clientes.AnyAsync(c => c.CNPJ == cliente.CNPJ))
            {
                return BadRequest("Cliente com este CNPJ já cadastrado.");
            }

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetCliente", new { id = cliente.Id }, cliente);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCliente(int id, Cliente cliente)
        {
            if (id != cliente.Id)
            {
                return BadRequest("O ID na URL não corresponde ao ID do cliente fornecido.");
            }

            var existingCliente = await _context.Clientes.FindAsync(id);
            if (existingCliente == null)
            {
                return NotFound($"Cliente com ID {id} não encontrado.");
            }

            // Validação de CNPJ único para outros clientes
            if (await _context.Clientes.AnyAsync(c => c.CNPJ == cliente.CNPJ && c.Id != id))
            {
                return BadRequest("Já existe um cliente cadastrado com este CNPJ.");
            }

            // Atualiza as propriedades do cliente existente com os valores do cliente recebido
            _context.Entry(existingCliente).CurrentValues.SetValues(cliente);
            // Optionally, you can update properties individually:
            // existingCliente.NomeEmpresa = cliente.NomeEmpresa;
            // existingCliente.CNPJ = cliente.CNPJ;
            // ... (other properties)

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClienteExists(id))
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

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
