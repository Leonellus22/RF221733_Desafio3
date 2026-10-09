using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BolsaTrabajo.Entities.Models;

// Usuario del sistema basado en IdentityUser (Administrador, Agente o Candidato)
public class Usuario : IdentityUser
{
    [Required]
    [StringLength(150)]
    public string NombreCompleto { get; set; } = string.Empty;

    // Rol principal del usuario (se sincroniza con la tabla AspNetUserRoles)
    [Required]
    [StringLength(20)]
    public string Rol { get; set; } = string.Empty;

    // Propiedades de navegación
    public ICollection<Plaza> PlazasPublicadas { get; set; } = new List<Plaza>();
    public ICollection<Postulacion> Postulaciones { get; set; } = new List<Postulacion>();
    public HojaDeVida? HojaDeVida { get; set; }
}
