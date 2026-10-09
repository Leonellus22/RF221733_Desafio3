using System.ComponentModel.DataAnnotations;
using BolsaTrabajo.Entities.Enums;

namespace BolsaTrabajo.Entities.Models;

// Oferta de empleo publicada por un agente de selección
public class Plaza
{
    [Key]
    public int IdPlaza { get; set; }

    [Required]
    [StringLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Descripcion { get; set; } = string.Empty;

    public DateTime FechaPublicacion { get; set; }

    public DateTime FechaCierre { get; set; }

    [Required]
    [StringLength(150)]
    public string Institucion { get; set; } = string.Empty;

    public EstadoPlaza Estado { get; set; } = EstadoPlaza.Abierta;

    // Referencia al usuario agente que publicó la plaza
    [Required]
    public string IdAgente { get; set; } = string.Empty;
    public Usuario? Agente { get; set; }

    public ICollection<Postulacion> Postulaciones { get; set; } = new List<Postulacion>();
}
