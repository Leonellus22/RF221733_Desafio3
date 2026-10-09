using System.ComponentModel.DataAnnotations;

namespace BolsaTrabajo.Entities.Dtos;

// DTO de lectura de un usuario
public class UsuarioDto
{
    public string Id { get; set; } = string.Empty;

    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Display(Name = "Correo")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [Display(Name = "Rol")]
    public string Rol { get; set; } = string.Empty;
}
