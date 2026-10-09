using System.ComponentModel.DataAnnotations;
using BolsaTrabajo.Entities.Enums;

namespace BolsaTrabajo.Entities.Dtos;

public class CambiarEstadoPlazaDto
{
    [Display(Name = "Estado")]
    [Required(ErrorMessage = "El {0} es obligatorio.")]
    [EnumDataType(typeof(EstadoPlaza), ErrorMessage = "El {0} no es válido.")]
    public EstadoPlaza Estado { get; set; }
}
