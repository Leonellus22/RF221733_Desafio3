using System.ComponentModel.DataAnnotations;
using BolsaTrabajo.Entities.Enums;

namespace BolsaTrabajo.Entities.Dtos;

// DTO que utiliza el agente para aprobar o rechazar una postulación
public class EvaluarPostulacionDto
{
    [Display(Name = "Estado")]
    [Required(ErrorMessage = "El {0} es obligatorio.")]
    [EnumDataType(typeof(EstadoPostulacion), ErrorMessage = "El {0} no es válido.")]
    public EstadoPostulacion Estado { get; set; }

    [Display(Name = "Observaciones")]
    [StringLength(500, ErrorMessage = "Las {0} no pueden tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }
}
