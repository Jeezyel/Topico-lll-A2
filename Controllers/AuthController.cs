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

        // 🔹 Injetando IConfiguration
        public AuthController(IConfiguration config, A2Context context)
        {
            _config = config;
            _context = context;
        }


        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            // Verifica se o modelo é válido
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("Username e Password são obrigatórios.");
            }

            // 1. Busca o usuário pelo Nome (ou Email, que é mais comum)
            var user = _context.Usuarios.Include(u => u.Role) // Incluir a Role para pegar o nome
                                        .FirstOrDefault(u => u.Nome == model.Username);

            if (user == null)
            {
                return Unauthorized("Credenciais inválidas."); // Não diga "Usuário não encontrado" por segurança
            }

            // 2. *** VALIDAÇÃO DE SENHA CORRETA COM BCRYPT ***
            // BCrypt.Verify(senha_digitada_texto_puro, hash_armazenado_no_banco)
            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.SenhaHash))
            {
                // Se a verificação falhar
                return Unauthorized("Credenciais inválidas.");
            }

            // 3. *** AUTENTICAÇÃO BEM-SUCEDIDA ***
            // Se chegou até aqui, o login e a senha estão corretos.

            // Obter o nome da Role (Papel)
            // Assumindo que você incluiu o relacionamento Role acima e que user.Role.Nome existe
            string roleName = user.Role?.Nome ?? "Usuário";

            var claims = new[]
            {
        // Use user.Nome ou user.Email para a ClaimTypes.Name
        new Claim(ClaimTypes.Name, user.Nome), 
        // Use o Role real do usuário no banco de dados
        new Claim(ClaimTypes.Role, roleName)
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            // Retorna o token
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }
    }
        public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
