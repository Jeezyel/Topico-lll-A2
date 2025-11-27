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
    public class JanelaHorarioController : ControllerBase
    {
        private readonly A2Context _context;
        public JanelaHorarioController(A2Context context) { _context = context; }

        [HttpGet] 
        public async Task<ActionResult<IEnumerable<JanelaHorario>>> 
            Get() => await _context.JanelasHorarias
            .ToListAsync();
        
        [HttpPost]
        public async Task<ActionResult<JanelaHorario>> Post(JanelaHorario janela)
        {
            _context.JanelasHorarias.Add(janela); await _context.SaveChangesAsync(); return Ok(janela);
        }
    }
}
