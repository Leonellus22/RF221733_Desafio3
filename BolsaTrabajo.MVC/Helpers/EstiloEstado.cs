using BolsaTrabajo.Entities.Enums;

namespace BolsaTrabajo.MVC.Helpers;

// Clases de Bootstrap utilizadas para mostrar los estados con colores
public static class EstiloEstado
{
    public static string Clase(EstadoPlaza estado) => estado switch
    {
        EstadoPlaza.Abierta => "bg-success",
        EstadoPlaza.EnEvaluacion => "bg-warning text-dark",
        _ => "bg-secondary"
    };

    public static string Clase(EstadoPostulacion estado) => estado switch
    {
        EstadoPostulacion.Aprobado => "bg-success",
        EstadoPostulacion.Rechazado => "bg-danger",
        _ => "bg-info text-dark"
    };
}
