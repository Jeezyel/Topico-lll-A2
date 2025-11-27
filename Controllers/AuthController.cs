using A2.Data; // Adicione para acessar o Contexto
using A2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
// Adicione para usar BCrypt
using BCrypt.Net;

namespace A2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly A2Context _context; // Injeta o Contexto do banco

        public AuthController(IConfiguration config, A2Context context)
        {
            _config = config;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            // 1. Busca o usuário no banco pelo e-mail (incluindo a Role)
            var usuario = await _context.Usuarios
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            // 2. Verifica se o usuário existe E se a senha bate com o hash
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.Password, usuario.SenhaHash))
            {
                return Unauthorized("E-mail ou senha inválidos.");
            }

            // 3. Se chegou aqui, o login é válido! Vamos gerar o token.

            // Cria as "Claims" (informações que vão dentro do token)
            var claims = new List<Claim>
            {
                // Identificador único do usuário
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                // Nome do usuário
                new Claim(ClaimTypes.Name, usuario.Nome),
                // E-mail
                new Claim(ClaimTypes.Email, usuario.Email)
            };

            // Adiciona a Role (papel) se ela existir
            if (usuario.Role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, usuario.Role.Nome));
            }

            // Pega a chave secreta da configuração
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Monta o token
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8), // Token válido por 8 horas
                signingCredentials: creds
            );

            // Retorna o token gerado
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                // Opcional: pode retornar dados básicos do usuário também
                usuario = new { usuario.Id, usuario.Nome, usuario.Email, Role = usuario.Role?.Nome }
            });
        }
    }

    // DTO para o login (mudei Username para Email para ficar mais claro)
    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}