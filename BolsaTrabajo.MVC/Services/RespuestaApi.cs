using System.Net;

namespace BolsaTrabajo.MVC.Services;

// Resultado de una llamada a la API
public class RespuestaApi<T>
{
    public bool Exito { get; set; }
    public HttpStatusCode Codigo { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public T? Datos { get; set; }

    // La API rechazó el token (sesión expirada o inválida)
    public bool SesionExpirada => Codigo == HttpStatusCode.Unauthorized;
}

// Archivo descargado desde la API
public class ArchivoApi
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public byte[] Contenido { get; set; } = [];
    public string TipoContenido { get; set; } = "application/octet-stream";
    public string NombreArchivo { get; set; } = "archivo";
}
