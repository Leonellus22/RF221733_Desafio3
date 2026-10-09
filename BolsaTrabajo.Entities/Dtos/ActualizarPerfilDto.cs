using System.ComponentModel.DataAnnotations;

namespace BolsaTrabajo.Entities.Dtos;

public class ActualizarPerfilDto
{
    [Display(Name = "Nombre completo")]
    [Required(ErrorMessage = "El {0} es obligatorio.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El {0} debe tener entre {2} y {1} caracteres.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "El {0} solo puede contener letras y espacios.")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Display(Name = "Teléfono")]
    [RegularExpression(@"^\d{4}-\d{4}$", ErrorMessage = "El {0} debe tener el formato 0000-0000.")]
    public string? Telefono { get; set; }
}
