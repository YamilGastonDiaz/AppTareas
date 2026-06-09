using AppTareas.Models;

namespace AppTareas.Servicios
{
    public interface IAlmacenadorArchivo
    {
        Task Borrar(string ruta, string contenedor);
        Task<AlmacenarArchivoResultado[]> Almacenar(string contenedor,
            IEnumerable<IFormFile> archivos);
    }
}
