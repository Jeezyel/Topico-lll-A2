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
    public class PontoDeParadaController : ControllerBase
    {
        private readonly A2Context _context;
        public PontoDeParadaController(A2Context context) { 
            _context = context; }

        [HttpGet] 
        public async Task<ActionResult<IEnumerable<PontoDeParada>>> Get() => await _context.PontosDeParada.ToListAsync();
        
        [HttpPost]
        public async Task<ActionResult<PontoDeParada>> Post(PontoDeParada ponto)
        {
            _context.PontosDeParada.Add(ponto); await _context.SaveChangesAsync(); return Ok(ponto);
        }
    }
}
