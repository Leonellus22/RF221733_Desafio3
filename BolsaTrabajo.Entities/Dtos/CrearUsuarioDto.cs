using System.ComponentModel.DataAnnotations;

namespace BolsaTrabajo.Entities.Dtos;

// Utilizado por el administrador para crear usuarios con cualquier rol
public class CrearUsuarioDto : RegistroDto
{
    [Display(Name = "Rol")]
    [Required(ErrorMessage = "El {0} es obligatorio.")]
    public string Rol { get; set; } = string.Empty;
}
