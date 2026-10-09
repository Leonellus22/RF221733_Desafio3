using System.ComponentModel.DataAnnotations;
using BolsaTrabajo.Entities.Enums;

namespace BolsaTrabajo.Entities.Dtos;

// DTO utilizado para crear, editar y mostrar plazas
public class PlazaDto : IValidatableObject
{
    public int IdPlaza { get; set; }

    [Display(Name = "Título")]
    [Required(ErrorMessage = "El {0} es obligatorio.")]
    [StringLength(150, MinimumLength = 5, ErrorMessage = "El {0} debe tener entre {2} y {1} caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Display(Name = "Descripción")]
    [Required(ErrorMessage = "La {0} es obligatoria.")]
    [StringLength(2000, MinimumLength = 20, ErrorMessage = "La {0} debe tener entre {2} y {1} caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Display(Name = "Fecha de publicación")]
    [Required(ErrorMessage = "La {0} es obligatoria.")]
    [DataType(DataType.Date)]
    public DateTime FechaPublicacion { get; set; } = DateTime.Today;

    [Display(Name = "Fecha de cierre")]
    [Required(ErrorMessage = "La {0} es obligatoria.")]
    [DataType(DataType.Date)]
    public DateTime FechaCierre { get; set; } = DateTime.Today.AddDays(15);

    [Display(Name = "Institución")]
    [Required(ErrorMessage = "La {0} es obligatoria.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "La {0} debe tener entre {2} y {1} caracteres.")]
    public string Institucion { get; set; } = string.Empty;

    [Display(Name = "Estado")]
    public EstadoPlaza Estado { get; set; } = EstadoPlaza.Abierta;

    // Datos de solo lectura que devuelve la API
    public string? IdAgente { get; set; }

    [Display(Name = "Agente")]
    public string? NombreAgente { get; set; }

    [Display(Name = "Postulaciones")]
    public int TotalPostulaciones { get; set; }

    // Validación de fechas a nivel de formulario
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FechaCierre.Date <= FechaPublicacion.Date)
        {
            yield return new ValidationResult(
                "La fecha de cierre debe ser posterior a la fecha de publicación.",
                [nameof(FechaCierre)]);
        }
    }
}
