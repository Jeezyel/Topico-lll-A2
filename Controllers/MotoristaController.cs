// using Microsoft.AspNetCore.Authorization; // Para controle de acesso por roles
using A2.Data;
using A2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace A2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize(Roles = "Administrador, Gerente")] // Apenas administradores e gerentes podem gerenciar motoristas
    public class MotoristaController : ControllerBase
    {
        private readonly A2Context _context;

        public MotoristaController(A2Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Motorista>>> GetMotoristas()
        {
            return await _context.Motoristas.Include(m => m.Usuario).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Motorista>> GetMotorista(int id)
        {
            var motorista = await _context.Motoristas
                                          .Include(m => m.Usuario)
                                          .FirstOrDefaultAsync(m => m.Id == id);

            if (motorista == null)
            {
                return NotFound();
            }

            return motorista;
        }

        [HttpPost]
        public async Task<ActionResult<Motorista>> PostMotorista(Motorista motorista)
        {
            if (await _context.Motoristas.AnyAsync(m => m.CPF == motorista.CPF))
            {
                return BadRequest("CPF já cadastrado.");
            }
            if (await _context.Motoristas.AnyAsync(m => m.CNH == motorista.CNH))
            {
                return BadRequest("CNH já cadastrada.");
            }

            if (motorista.Usuario != null)
            {
                if (string.IsNullOrWhiteSpace(motorista.Usuario.Email) || string.IsNullOrWhiteSpace(motorista.Usuario.SenhaHash))
                {
                    return BadRequest("Email e Senha do usuário são obrigatórios para criar um usuário para o motorista.");
                }

                var roleMotorista = await _context.Roles.FirstOrDefaultAsync(r => r.Nome == "Motorista");
                if (roleMotorista == null)
                {
                    return BadRequest("Role 'Motorista' não encontrada no sistema. Cadastre-a primeiro.");
                }

                motorista.Usuario.SenhaHash = motorista.Usuario.SenhaHash;
                motorista.Usuario.RoleId = roleMotorista.Id;
                motorista.Usuario.Role = roleMotorista; 

            }

            _context.Motoristas.Add(motorista);
            await _context.SaveChangesAsync();

            await _context.Entry(motorista).Reference(m => m.Usuario).LoadAsync();

            return CreatedAtAction("GetMotorista", new { id = motorista.Id }, motorista);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutMotorista(int id, Motorista motorista)
        {
            if (id != motorista.Id)
            {
                return BadRequest();
            }

            if (await _context.Motoristas.AnyAsync(m => m.CPF == motorista.CPF && m.Id != motorista.Id))
            {
                return BadRequest("CPF já cadastrado para outro motorista.");
            }
            if (await _context.Motoristas.AnyAsync(m => m.CNH == motorista.CNH && m.Id != motorista.Id))
            {
                return BadRequest("CNH já cadastrada para outro motorista.");
            }

            _context.Entry(motorista).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MotoristaExists(id))
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
        public async Task<IActionResult> DeleteMotorista(int id)
        {
            var motorista = await _context.Motoristas.Include(m => m.Usuario).FirstOrDefaultAsync(m => m.Id == id);
            if (motorista == null)
            {
                return NotFound();
            }

            _context.Motoristas.Remove(motorista);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MotoristaExists(int id)
        {
            return _context.Motoristas.Any(e => e.Id == id);
        }
    }
}