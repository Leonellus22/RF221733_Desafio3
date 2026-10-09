using System.ComponentModel.DataAnnotations;

namespace BolsaTrabajo.Entities.Models;

// Hoja de vida (CV) del candidato
public class HojaDeVida
{
    [Key]
    public int IdCV { get; set; }

    [Required]
    public string IdUsuario { get; set; } = string.Empty;
    public Usuario? Usuario { get; set; }

    [Required]
    [StringLength(2000)]
    public string FormacionAcademica { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string ExperienciaLaboral { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Competencias { get; set; } = string.Empty;

    // Ruta relativa del documento PDF almacenado en el servidor
    [StringLength(300)]
    public string? ArchivoCV { get; set; }
}
