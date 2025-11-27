using A2.Data;
using A2.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;

namespace A2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly A2Context _context;

        public UsuarioController(A2Context context)
        {
            _context = context;
        }

        // GET: api/Usuario
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            // Inclui a Role na resposta para sabermos o perfil de cada usuário
            return await _context.Usuarios.Include(u => u.Role).ToListAsync();
        }

        // GET: api/Usuario/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                                        .Include(u => u.Role)
                                        .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
        }

        // POST: api/Usuario
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            // 1. Validação de e-mail duplicado
            if (await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email))
            {
                return BadRequest("E-mail já cadastrado.");
            }

            // 2. Validar se a Role existe
            if (!await _context.Roles.AnyAsync(r => r.Id == usuario.RoleId))
            {
                return BadRequest("Role (Perfil) inválida ou inexistente. Crie a Role primeiro.");
            }

            // --- CRIPTOGRAFIA DA SENHA ---
            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(usuario.SenhaHash);
            // ----------------------------------------------

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Trazemos o objeto Role completo para o retorno ficar bonito no JSON
            await _context.Entry(usuario).Reference(u => u.Role).LoadAsync();

            return CreatedAtAction("GetUsuario", new { id = usuario.Id }, usuario);
        }

        // PUT: api/Usuario/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return BadRequest("O ID na URL não corresponde ao ID do usuário fornecido.");
            }

            var existingUsuario = await _context.Usuarios
                                                .AsNoTracking() // Carrega para não rastrear e poder anexar 'usuario' depois
                                                .FirstOrDefaultAsync(u => u.Id == id);

            if (existingUsuario == null)
            {
                return NotFound($"Usuário com ID {id} não encontrado.");
            }

            // Validar se a Role existe se o RoleId for alterado
            if (existingUsuario.RoleId != usuario.RoleId && !await _context.Roles.AnyAsync(r => r.Id == usuario.RoleId))
            {
                return BadRequest("Role (Perfil) inválida ou inexistente. Crie a Role primeiro.");
            }

            // Acompanha a entidade 'usuario' e marca como modificada
            _context.Entry(usuario).State = EntityState.Modified;

            // Lógica para atualização de senha:
            if (!string.IsNullOrWhiteSpace(usuario.SenhaHash))
            {
                // Assume que a SenhaHash recebida é a senha em texto puro que precisa ser criptografada.
                usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(usuario.SenhaHash);
            }
            else
            {
                // Se a SenhaHash não foi fornecida, mantém o hash existente do banco.
                _context.Entry(usuario).Property(u => u.SenhaHash).IsModified = false;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
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

        // DELETE: api/Usuario/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}