using System.ComponentModel.DataAnnotations;

namespace AppTareas.Models
{
    public class TareaEditarDto
    {
        [Required]
        [StringLength(250)]
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
    }
}
