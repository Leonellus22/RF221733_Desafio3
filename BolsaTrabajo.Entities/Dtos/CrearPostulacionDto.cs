using System.ComponentModel.DataAnnotations;

namespace BolsaTrabajo.Entities.Dtos;

public class CrearPostulacionDto
{
    [Required(ErrorMessage = "La plaza es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "La plaza no es válida.")]
    public int IdPlaza { get; set; }
}
