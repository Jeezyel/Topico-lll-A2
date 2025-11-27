// using Microsoft.AspNetCore.Authorization; // Para controle de acesso por roles
using A2.Data;
using A2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace A2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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

                motorista.Usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(motorista.Usuario.SenhaHash);
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
                return BadRequest("O ID na URL não corresponde ao ID do motorista fornecido.");
            }

            var existingMotorista = await _context.Motoristas
                                                .Include(m => m.Usuario) // Inclui o usuário para possível atualização
                                                .FirstOrDefaultAsync(m => m.Id == id);

            if (existingMotorista == null)
            {
                return NotFound($"Motorista com ID {id} não encontrado.");
            }

            // Validação de CPF e CNH únicos para outros motoristas
            if (await _context.Motoristas.AnyAsync(m => m.CPF == motorista.CPF && m.Id != id))
            {
                return BadRequest("CPF já cadastrado para outro motorista.");
            }
            if (await _context.Motoristas.AnyAsync(m => m.CNH == motorista.CNH && m.Id != id))
            {
                return BadRequest("CNH já cadastrada para outro motorista.");
            }

            // Atualiza as propriedades escalares do motorista existente
            _context.Entry(existingMotorista).CurrentValues.SetValues(motorista);

            // === Lógica para atualização do Usuário associado ===
            if (motorista.Usuario != null)
            {
                if (existingMotorista.Usuario == null)
                {
                    // Se não havia usuário e um está sendo fornecido, cria um novo
                    var roleMotorista = await _context.Roles.FirstOrDefaultAsync(r => r.Nome == "Motorista");
                    if (roleMotorista == null)
                    {
                        return BadRequest("Role 'Motorista' não encontrada no sistema. Cadastre-a primeiro.");
                    }

                    var newUser = new Usuario
                    {
                        Nome = motorista.Nome, // Ou motorista.Usuario.Nome, dependendo da regra
                        Email = motorista.Usuario.Email,
                        SenhaHash = BCrypt.Net.BCrypt.HashPassword(motorista.Usuario.SenhaHash),
                        RoleId = roleMotorista.Id,
                        Role = roleMotorista
                    };
                    _context.Usuarios.Add(newUser);
                    existingMotorista.Usuario = newUser;
                }
                else
                {
                    // Se já havia usuário, atualiza suas propriedades
                    existingMotorista.Usuario.Nome = motorista.Usuario.Nome;
                    existingMotorista.Usuario.Email = motorista.Usuario.Email;

                    // Apenas atualiza a senha se uma nova senha for fornecida
                    if (!string.IsNullOrWhiteSpace(motorista.Usuario.SenhaHash) && 
                        !BCrypt.Net.BCrypt.Verify(motorista.Usuario.SenhaHash, existingMotorista.Usuario.SenhaHash)) // Only hash if password changed
                    {
                        existingMotorista.Usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(motorista.Usuario.SenhaHash);
                    }
                    _context.Entry(existingMotorista.Usuario).State = EntityState.Modified;
                }
            } else if (existingMotorista.Usuario != null) {
                // Se um usuário existia e não foi fornecido na atualização, pode-se decidir removê-lo
                // Ou manter, definindo UsuarioId como null no Motorista e removendo o usuário
                _context.Usuarios.Remove(existingMotorista.Usuario);
                existingMotorista.UsuarioId = null;
            }


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

            if (motorista.Usuario != null)
            {
                _context.Usuarios.Remove(motorista.Usuario);
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