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
    public class IncidenciaRotaController : ControllerBase
    {
        private readonly A2Context _context;
        public IncidenciaRotaController(A2Context context) { _context = context; }

        [HttpGet] public async Task<ActionResult<IEnumerable<IncidenciaRota>>> Get() => await _context.IncidenciasRota.ToListAsync();
        [HttpPost]
        public async Task<ActionResult<IncidenciaRota>> Post(IncidenciaRota incidencia)
        {
            _context.IncidenciasRota.Add(incidencia); await _context.SaveChangesAsync(); return Ok(incidencia);
        }
    }
}
