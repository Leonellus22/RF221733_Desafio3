using System.ComponentModel.DataAnnotations;

namespace BolsaTrabajo.Entities.Dtos;

// Credenciales para iniciar sesión (mismo formato que espera el endpoint /api/auth/login de Identity)
public class LoginDto
{
    [Display(Name = "Correo electrónico")]
    [Required(ErrorMessage = "El {0} es obligatorio.")]
    [EmailAddress(ErrorMessage = "El {0} no tiene un formato válido.")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Contraseña")]
    [Required(ErrorMessage = "La {0} es obligatoria.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
