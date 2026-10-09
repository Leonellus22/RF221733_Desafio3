using System.ComponentModel.DataAnnotations;
using BolsaTrabajo.Entities.Enums;

namespace BolsaTrabajo.Entities.Models;

// Aplicación de un candidato a una plaza
public class Postulacion
{
    [Key]
    public int IdPostulacion { get; set; }

    public int IdPlaza { get; set; }
    public Plaza? Plaza { get; set; }

    [Required]
    public string IdCandidato { get; set; } = string.Empty;
    public Usuario? Candidato { get; set; }

    public DateTime FechaPostulacion { get; set; }

    public EstadoPostulacion Estado { get; set; } = EstadoPostulacion.EnRevision;

    // Comentario que deja el agente al evaluar la postulación
    [StringLength(500)]
    public string? Observaciones { get; set; }
}
