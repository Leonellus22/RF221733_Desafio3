using System.ComponentModel.DataAnnotations;

namespace BolsaTrabajo.Entities.Enums;

public enum EstadoPostulacion
{
    [Display(Name = "En revisión")]
    EnRevision = 1,

    [Display(Name = "Aprobado")]
    Aprobado = 2,

    [Display(Name = "Rechazado")]
    Rechazado = 3
}
