using System.ComponentModel.DataAnnotations;

namespace BolsaTrabajo.Entities.Dtos;

public class CambiarRolDto
{
    [Display(Name = "Rol")]
    [Required(ErrorMessage = "El {0} es obligatorio.")]
    public string Rol { get; set; } = string.Empty;
}
