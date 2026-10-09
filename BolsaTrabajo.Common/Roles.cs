namespace BolsaTrabajo.Common;

// Nombres de los roles utilizados en todo el sistema
public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Agente = "Agente";
    public const string Candidato = "Candidato";

    // Combinaciones utilizadas en los atributos [Authorize(Roles = "...")]
    public const string AgenteOAdministrador = Agente + "," + Administrador;

    public static readonly string[] Todos = [Administrador, Agente, Candidato];
}
