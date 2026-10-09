using System.ComponentModel.DataAnnotations;

namespace BolsaTrabajo.Entities.Dtos;

public class HojaDeVidaDto
{
    public int IdCV { get; set; }

    public string? IdUsuario { get; set; }

    [Display(Name = "Candidato")]
    public string? NombreCandidato { get; set; }

    [Display(Name = "Correo")]
    public string? EmailCandidato { get; set; }

    [Display(Name = "Formación académica")]
    [Required(ErrorMessage = "La {0} es obligatoria.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "La {0} debe tener entre {2} y {1} caracteres.")]
    public string FormacionAcademica { get; set; } = string.Empty;

    [Display(Name = "Experiencia laboral")]
    [Required(ErrorMessage = "La {0} es obligatoria.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "La {0} debe tener entre {2} y {1} caracteres.")]
    public string ExperienciaLaboral { get; set; } = string.Empty;

    [Display(Name = "Competencias")]
    [Required(ErrorMessage = "Las {0} son obligatorias.")]
    [StringLength(1000, MinimumLength = 3, ErrorMessage = "Las {0} deben tener entre {2} y {1} caracteres.")]
    public string Competencias { get; set; } = string.Empty;

    // Ruta del PDF (la asigna el servidor al subir el archivo)
    [Display(Name = "Archivo CV")]
    public string? ArchivoCV { get; set; }

    public bool TieneArchivo => !string.IsNullOrWhiteSpace(ArchivoCV);
}
