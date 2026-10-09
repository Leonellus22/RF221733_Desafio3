namespace BolsaTrabajo.Common;

// Configuración para el almacenamiento de las hojas de vida (sección "ArchivoSettings" del appsettings.json)
public class ArchivoSettings
{
    public string CarpetaCV { get; set; } = "Archivos/CV";
    public int TamanoMaximoMB { get; set; } = 5;
}
