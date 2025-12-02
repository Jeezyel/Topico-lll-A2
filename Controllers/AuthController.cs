using A2.Data;
using A2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace A2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly A2Context _context;

        public AuthController(IConfiguration config, A2Context context)
        {
            _config = config;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            // Validação básica de entrada
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("E-mail e Senha são obrigatórios.");
            }

            // Busca o usuário pelo e-mail no banco de dados
            var usuario = await _context.Usuarios
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            // Verifica se o usuário existe e se a senha bate com o hash
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.Password, usuario.SenhaHash))
            {
                return Unauthorized("E-mail ou senha inválidos.");
            }

            // Criação das Claims (Informações que irão dentro do token)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email)
            };

            // Adiciona a Role se ela existir
            if (usuario.Role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, usuario.Role.Nome));
            }

            // Geração da chave de assinatura
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Criação do Token JWT
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8), // Token válido por 8 horas
                signingCredentials: creds
            );

            // Retorna o token gerado e dados básicos do usuário
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                usuario = new { usuario.Id, usuario.Nome, usuario.Email, Role = usuario.Role?.Nome }
            });
        }
    }

    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}