using System.Security.Claims;

namespace AppTareas.Servicios
{
    public interface IServicioUsuarios
    {
        string ObtenerUsuarioId();
    }

    public class ServicioUsuarios : IServicioUsuarios
    {
        private HttpContext _httpContext;

        public ServicioUsuarios(IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext.HttpContext;
        }

        public string ObtenerUsuarioId()
        {
            if (_httpContext.User.Identity.IsAuthenticated)
            {
                var id = _httpContext.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier)
                    .FirstOrDefault();

                return id.Value;
            }
            else
            {
                throw new Exception("El usuario no esta autenticado");
            }
        }
    }
}
