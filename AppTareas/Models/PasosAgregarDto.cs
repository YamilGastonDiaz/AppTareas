using System.ComponentModel.DataAnnotations;

namespace AppTareas.Models
{
    public class PasosAgregarDto
    {
        [Required]
        public string Descripcion { get; set; }
        public bool Realizado { get; set; }
    }
}
