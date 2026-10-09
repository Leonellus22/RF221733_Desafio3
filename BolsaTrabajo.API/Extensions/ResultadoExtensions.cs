using System.Security.Claims;
using BolsaTrabajo.Common;
using Microsoft.AspNetCore.Mvc;

namespace BolsaTrabajo.API.Extensions;

public static class ResultadoExtensions
{
    // Convierte el resultado de la capa de negocio en la respuesta HTTP correspondiente
    public static IActionResult ToActionResult<T>(this ControllerBase controller, ResultadoOperacion<T> resultado)
    {
        var cuerpo = new { mensaje = resultado.Mensaje };
        return resultado.Tipo switch
        {
            TipoResultado.Exito => controller.Ok(resultado.Datos),
            TipoResultado.NoEncontrado => controller.NotFound(cuerpo),
            TipoResultado.NoAutorizado => controller.Unauthorized(cuerpo),
            TipoResultado.Prohibido => controller.StatusCode(StatusCodes.Status403Forbidden, cuerpo),
            TipoResultado.Conflicto => controller.Conflict(cuerpo),
            _ => controller.BadRequest(cuerpo)
        };
    }

    // Id del usuario autenticado (claim NameIdentifier del token)
    public static string? ObtenerIdUsuario(this ControllerBase controller)
    {
        return controller.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public static bool EsAdministrador(this ControllerBase controller)
    {
        return controller.HttpContext?.User?.IsInRole(Roles.Administrador) ?? false;
    }
}
