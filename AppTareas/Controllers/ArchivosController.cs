using AppTareas.Data;
using AppTareas.Entidades;
using AppTareas.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppTareas.Controllers
{
    [Route("api/archivos")]
    public class ArchivosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAlmacenadorArchivo _almacenador;
        private readonly IServicioUsuarios _servicioUsuarios;
        private readonly string contenedor = "archivosadjuntos";

        public ArchivosController(ApplicationDbContext context, IAlmacenadorArchivo almacenador,
            IServicioUsuarios servicioUsuarios)
        {
            _context = context;
            _almacenador = almacenador;
            _servicioUsuarios = servicioUsuarios;
        }

        [HttpPost("{tareaId:int}")]
        public async Task<ActionResult<IEnumerable<ArchivoAdjunto>>> Post(int tareaId,
            [FromForm] IEnumerable<IFormFile> archivos)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

            var tarea = await _context.Tareas.FirstOrDefaultAsync(t => t.Id == tareaId);

            if (tarea is null)
            {
                return NotFound();
            }

            if (tarea.UsuarioCreacionId != usuarioId)
            {
                return Forbid();
            }

            var existenArchivos = await _context.ArchivoAdjuntos.AnyAsync(a => a.TareaId == tareaId);

            var oredenMayor = 0;

            if (existenArchivos)
            {
                oredenMayor = await _context.ArchivoAdjuntos.Where(a => a.TareaId == tareaId).Select(a => a.Orden).MaxAsync();
            }

            var resultado = await _almacenador.Almacenar(contenedor, archivos);

            var archivosAdjuntos = resultado.Select((resultado, indice) => new ArchivoAdjunto
            {
                TareaId = tareaId,
                FechaCreacion = DateTime.UtcNow,
                Url = resultado.Url,
                Titulo = resultado.Titulo,
                Orden = oredenMayor + indice + 1
            }).ToList();

            _context.AddRange(archivosAdjuntos);
            await _context.SaveChangesAsync();

            return archivosAdjuntos.ToList();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] string titulo)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

            var archivos = await _context.ArchivoAdjuntos.Include(a => a.Tarea).FirstOrDefaultAsync(a => a.Id == id);

            if (archivos is null)
            {
                return NotFound();
            }

            if (archivos.Tarea.UsuarioCreacionId != usuarioId)
            {
                return Forbid();
            }

            archivos.Titulo = titulo;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

            var archivos = await _context.ArchivoAdjuntos.Include(a => a.Tarea).FirstOrDefaultAsync(a => a.Id == id);

            if (archivos is null)
            {
                return NotFound();
            }

            if (archivos.Tarea.UsuarioCreacionId != usuarioId)
            {
                return Forbid();
            }

            _context.Remove(archivos);
            await _context.SaveChangesAsync();
            await _almacenador.Borrar(archivos.Url, contenedor);

            return Ok();
        }

        [HttpPost("ordenar/{tareaId:int}")]
        public async Task<IActionResult> Ordenar(int tareaId, [FromBody] Guid[] ids)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

            var tarea = await _context.Tareas.FirstOrDefaultAsync(t => t.Id == tareaId && t.UsuarioCreacionId == usuarioId);

            if (tarea is null)
            {
                return NotFound();
            }

            var archivos = await _context.ArchivoAdjuntos.Where(p => p.TareaId == tareaId).ToListAsync();

            var archivoIds = archivos.Select(p => p.Id);

            var idsArchivosNoPertenecen = ids.Except(archivoIds).ToList();

            if (idsArchivosNoPertenecen.Any())
            {
                return BadRequest("No todos los archivos estan presentes");
            }

            var archivoDisccionario = archivos.ToDictionary(p => p.Id);

            for (int i = 0; i < ids.Length; i++)
            {
                var archivoId = ids[i];
                var archivo = archivoDisccionario[archivoId];
                archivo.Orden = i + 1;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
