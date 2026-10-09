using System.Security.Claims;
using BolsaTrabajo.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace BolsaTrabajo.MVC.Controllers;

// Funcionalidad común para los controladores que consumen la API
public abstract class BaseController : Controller
{
    protected string? IdUsuario => User.FindFirstValue(ClaimTypes.NameIdentifier);

    protected void MensajeExito(string mensaje) => TempData["Exito"] = mensaje;

    protected void MensajeError(string mensaje) => TempData["Error"] = mensaje;

    // Si la API respondió 401 se cierra la sesión local y se envía al login
    protected IActionResult? VerificarSesion<T>(RespuestaApi<T> respuesta)
    {
        return respuesta.SesionExpirada ? RedirectToAction("SesionExpirada", "Cuenta") : null;
    }
}
