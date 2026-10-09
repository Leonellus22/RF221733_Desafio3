using System.ComponentModel.DataAnnotations;
using BolsaTrabajo.Entities.Enums;

namespace BolsaTrabajo.Entities.Dtos;

// DTO de lectura de una postulación
public class PostulacionDto
{
    public int IdPostulacion { get; set; }

    public int IdPlaza { get; set; }

    [Display(Name = "Plaza")]
    public string TituloPlaza { get; set; } = string.Empty;

    [Display(Name = "Institución")]
    public string Institucion { get; set; } = string.Empty;

    [Display(Name = "Estado de la plaza")]
    public EstadoPlaza EstadoPlaza { get; set; }

    public string IdCandidato { get; set; } = string.Empty;

    [Display(Name = "Candidato")]
    public string NombreCandidato { get; set; } = string.Empty;

    [Display(Name = "Correo")]
    public string EmailCandidato { get; set; } = string.Empty;

    [Display(Name = "Fecha de postulación")]
    public DateTime FechaPostulacion { get; set; }

    [Display(Name = "Estado")]
    public EstadoPostulacion Estado { get; set; }

    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }
}
