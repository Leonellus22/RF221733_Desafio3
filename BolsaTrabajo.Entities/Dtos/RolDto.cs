namespace BolsaTrabajo.Entities.Dtos;

// Rol con la cantidad de usuarios asignados
public class RolDto
{
    public string Nombre { get; set; } = string.Empty;
    public int TotalUsuarios { get; set; }
}
