using System.ComponentModel.DataAnnotations;

namespace BolsaTrabajo.Entities.Dtos;

// Datos personales para el registro de un candidato
public class RegistroDto
{
    [Display(Name = "Nombre completo")]
    [Required(ErrorMessage = "El {0} es obligatorio.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El {0} debe tener entre {2} y {1} caracteres.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "El {0} solo puede contener letras y espacios.")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Display(Name = "Correo electrónico")]
    [Required(ErrorMessage = "El {0} es obligatorio.")]
    [EmailAddress(ErrorMessage = "El {0} no tiene un formato válido.")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Teléfono")]
    [RegularExpression(@"^\d{4}-\d{4}$", ErrorMessage = "El {0} debe tener el formato 0000-0000.")]
    public string? Telefono { get; set; }

    [Display(Name = "Contraseña")]
    [Required(ErrorMessage = "La {0} es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La {0} debe tener al menos {2} caracteres.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Confirmar contraseña")]
    [Required(ErrorMessage = "Debe confirmar la contraseña.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmarPassword { get; set; } = string.Empty;
}
