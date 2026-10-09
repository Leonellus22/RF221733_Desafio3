using BolsaTrabajo.Common;
using BolsaTrabajo.Entities.Dtos;
using BolsaTrabajo.Entities.Enums;
using BolsaTrabajo.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolsaTrabajo.MVC.Controllers;

// Postulaciones de los candidatos y evaluación por parte de los agentes
[Authorize]
public class PostulacionesController(ApiClient api) : BaseController
{
    // POST: Postulaciones/Postular
    [Authorize(Roles = Roles.Candidato)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Postular(int idPlaza)
    {
        var respuesta = await api.PostAsync<PostulacionDto>("api/Postulaciones", new CrearPostulacionDto { IdPlaza = idPlaza });
        if (VerificarSesion(respuesta) is { } redireccion) return redireccion;

        if (!respuesta.Exito)
        {
            MensajeError(respuesta.Mensaje);
            return RedirectToAction("Details", "Plazas", new { id = idPlaza });
        }

        MensajeExito("Su postulación fue registrada. Puede dar seguimiento en \"Mis postulaciones\".");
        return RedirectToAction(nameof(MisPostulaciones));
    }

    // GET: Postulaciones/MisPostulaciones
    [Authorize(Roles = Roles.Candidato)]
    public async Task<IActionResult> MisPostulaciones()
    {
        var respuesta = await api.GetAsync<List<PostulacionDto>>("api/Postulaciones/mis-postulaciones");
        if (VerificarSesion(respuesta) is { } redireccion) return redireccion;
        if (!respuesta.Exito) ViewBag.Error = respuesta.Mensaje;
        return View(respuesta.Datos ?? []);
    }

    // GET: Postulaciones  -> monitoreo de todas las postulaciones (administrador)
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Index(EstadoPostulacion? estado)
    {
        var url = estado.HasValue ? $"api/Postulaciones?estado={estado}" : "api/Postulaciones";
        var respuesta = await api.GetAsync<List<PostulacionDto>>(url);
        if (VerificarSesion(respuesta) is { } redireccion) return redireccion;
        if (!respuesta.Exito) ViewBag.Error = respuesta.Mensaje;
        ViewBag.Estado = estado;
        return View(respuesta.Datos ?? []);
    }

    // POST: Postulaciones/Evaluar/5
    [Authorize(Roles = Roles.AgenteOAdministrador)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Evaluar(int id, int idPlaza, EvaluarPostulacionDto dto)
    {
        if (!ModelState.IsValid)
        {
            MensajeError(string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return RedirectToAction("Postulaciones", "Plazas", new { id = idPlaza });
        }

        var respuesta = await api.PutAsync<PostulacionDto>($"api/Postulaciones/{id}/estado", dto);
        if (VerificarSesion(respuesta) is { } redireccion) return redireccion;

        if (respuesta.Exito)
            MensajeExito($"La postulación de {respuesta.Datos!.NombreCandidato} quedó como \"{dto.Estado.ObtenerNombre()}\".");
        else
            MensajeError(respuesta.Mensaje);

        return RedirectToAction("Postulaciones", "Plazas", new { id = idPlaza });
    }
}
