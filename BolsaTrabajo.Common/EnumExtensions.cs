using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BolsaTrabajo.Common;

public static class EnumExtensions
{
    // Devuelve el texto definido en [Display(Name = "...")] o el nombre del valor del enum
    public static string ObtenerNombre(this Enum valor)
    {
        var miembro = valor.GetType().GetMember(valor.ToString()).FirstOrDefault();
        var display = miembro?.GetCustomAttribute<DisplayAttribute>();
        return display?.GetName() ?? valor.ToString();
    }
}
