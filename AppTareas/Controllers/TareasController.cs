using AppTareas.Data;
using AppTareas.Entidades;
using AppTareas.Models;
using AppTareas.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppTareas.Controllers
{
    [Route("api/tareas")]
    public class TareasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IServicioUsuarios _servicioUsuarios;

        public TareasController(ApplicationDbContext context, IServicioUsuarios servicioUsuarios)
        {
            _context = context;
            _servicioUsuarios = servicioUsuarios;
        }

        [HttpGet]
        public async Task<ActionResult<List<TareaDto>>> Get()
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

            var tareas = await _context.Tareas.Where(t => t.UsuarioCreacionId == usuarioId)
                .OrderBy(t => t.Orden)
                .Select(t => new TareaDto
                {
                    Id = t.Id,
                    Nombre = t.Nombre
                })
                .ToListAsync();

            return tareas;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Tarea>> Get(int id)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

            var tarea = await _context.Tareas.FirstOrDefaultAsync(t => t.Id == id && t.UsuarioCreacionId == usuarioId);

            if (tarea is null)
            {
                return NotFound();
            }

            return tarea;
        }

        [HttpPost]
        public async Task<ActionResult<Tarea>> Post([FromBody] string titulo)
        {
            var userId = _servicioUsuarios.ObtenerUsuarioId();

            var tareaExiste = await _context.Tareas.AnyAsync(t => t.UsuarioCreacionId == userId);

            var ordenMayor = 0;

            if (tareaExiste)
            {
                ordenMayor = await _context.Tareas.Where(t => t.UsuarioCreacionId == userId)
                    .Select(t => t.Orden).MaxAsync();
            }

            var tarea = new Tarea
            {
                Nombre = titulo,
                UsuarioCreacionId = userId,
                FechaCreacion = DateTime.UtcNow,
                Orden = ordenMayor + 1
            };

            _context.Add(tarea);
            await _context.SaveChangesAsync();

            return tarea;
        }

        [HttpPost("ordenar")]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

            var tareas = await _context.Tareas.Where(t => t.UsuarioCreacionId == usuarioId).ToListAsync();

            var tareaId = tareas.Select(t => t.Id);

            var idTareaNoPertenecenAlUser = ids.Except(tareaId).ToList();

            if (idTareaNoPertenecenAlUser.Any())
            {
                return Forbid();
            }

            var tareasDiccionario = tareas.ToDictionary(d => d.Id);

            for (int i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                var tarea = tareasDiccionario[id];
                tarea.Orden = i + 1;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> EditarTarea(int id, [FromBody] TareaEditarDto dto)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

            var tarea = await _context.Tareas.FirstOrDefaultAsync(t => t.Id == id && t.UsuarioCreacionId == usuarioId);

            if (tarea is null)
            {
                return NotFound();
            }

            tarea.Nombre = dto.Nombre;
            tarea.Descripcion = dto.Descripcion;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

            var tarea = await _context.Tareas.FirstOrDefaultAsync(t => t.Id == id && t.UsuarioCreacionId == usuarioId);

            if (tarea is null)
            {
                return NotFound();
            }

            _context.Remove(tarea);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
