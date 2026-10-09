using BolsaTrabajo.Common;
using BolsaTrabajo.Entities.Dtos;
using BolsaTrabajo.Entities.Enums;
using BolsaTrabajo.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolsaTrabajo.MVC.Controllers;

// Publicación y gestión de plazas
[Authorize(Roles = Roles.AgenteOAdministrador)]
public class PlazasController(ApiClient api) : BaseController
{
    // GET: Plazas/Details/5  -> detalle público de una plaza
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var respuesta = await api.GetAsync<PlazaDto>($"api/Plazas/{id}");
        if (!respuesta.Exito || respuesta.Datos == null)
        {
            MensajeError(respuesta.Mensaje);
            return RedirectToAction("Index", "Home");
        }

        // Si es candidato, se verifica si ya se postuló a la plaza
        if (User.IsInRole(Roles.Candidato))
        {
            var postulaciones = await api.GetAsync<List<PostulacionDto>>("api/Postulaciones/mis-postulaciones");
            ViewBag.YaPostulado = postulaciones.Datos?.Any(p => p.IdPlaza == id) ?? false;
        }

        return View(respuesta.Datos);
    }

    // GET: Plazas/MisPlazas  -> plazas publicadas por el agente
    [Authorize(Roles = Roles.Agente)]
    public async Task<IActionResult> MisPlazas()
    {
        var respuesta = await api.GetAsync<List<PlazaDto>>("api/Plazas/mis-plazas");
        if (VerificarSesion(respuesta) is { } redireccion) return redireccion;
        if (!respuesta.Exito) ViewBag.Error = respuesta.Mensaje;
        ViewBag.Titulo = "Mis plazas publicadas";
        return View("Lista", respuesta.Datos ?? []);
    }

    // GET: Plazas/Todas  -> monitoreo de todas las plazas (administrador)
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Todas(EstadoPlaza? estado)
    {
        var respuesta = await api.GetAsync<List<PlazaDto>>("api/Plazas");
        if (VerificarSesion(respuesta) is { } redireccion) return redireccion;
        if (!respuesta.Exito) ViewBag.Error = respuesta.Mensaje;

        var plazas = respuesta.Datos ?? [];
        if (estado.HasValue)
            plazas = plazas.Where(p => p.Estado == estado.Value).ToList();

        ViewBag.Titulo = "Monitoreo de plazas";
        ViewBag.Estado = estado;
        return View("Lista", plazas);
    }

    // GET: Plazas/Create
    [Authorize(Roles = Roles.Agente)]
    public IActionResult Create()
    {
        return View(new PlazaDto());
    }

    // POST: Plazas/Create
    [Authorize(Roles = Roles.Agente)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PlazaDto plaza)
    {
        if (!ModelState.IsValid)
            return View(plaza);

        var respuesta = await api.PostAsync<PlazaDto>("api/Plazas", plaza);
        if (VerificarSesion(respuesta) is { } redireccion) return redireccion;
        if (!respuesta.Exito)
        {
            ModelState.AddModelError(string.Empty, respuesta.Mensaje);
            return View(plaza);
        }

        MensajeExito("Plaza publicada correctamente.");
        return RedirectToAction(nameof(MisPlazas));
    }

    // GET: Plazas/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var respuesta = await api.GetAsync<PlazaDto>($"api/Plazas/{id}");
        if (!respuesta.Exito || respuesta.Datos == null)
        {
            MensajeError(respuesta.Mensaje);
            return RedirigirALista();
        }
        return View(respuesta.Datos);
    }

    // POST: Plazas/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PlazaDto plaza)
    {
        if (id != plaza.IdPlaza)
            return NotFound();

        if (!ModelState.IsValid)
            return View(plaza);

        var respuesta = await api.PutAsync<PlazaDto>($"api/Plazas/{id}", plaza);
        if (VerificarSesion(respuesta) is { } redireccion) return redireccion;
        if (!respuesta.Exito)
        {
            ModelState.AddModelError(string.Empty, respuesta.Mensaje);
            return View(plaza);
        }

        MensajeExito("Plaza actualizada correctamente.");
        return RedirigirALista();
    }

    // POST: Plazas/CambiarEstado/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, EstadoPlaza estado, string? volver)
    {
        var respuesta = await api.PutAsync<PlazaDto>($"api/Plazas/{id}/estado", new CambiarEstadoPlazaDto { Estado = estado });
        if (VerificarSesion(respuesta) is { } redireccion) return redireccion;

        if (respuesta.Exito)
            MensajeExito($"La plaza ahora está en estado \"{estado.ObtenerNombre()}\".");
        else
            MensajeError(respuesta.Mensaje);

        return volver == "postulaciones" ? RedirectToAction(nameof(Postulaciones), new { id }) : RedirigirALista();
    }

    // GET: Plazas/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var respuesta = await api.GetAsync<PlazaDto>($"api/Plazas/{id}");
        if (!respuesta.Exito || respuesta.Datos == null)
        {
            MensajeError(respuesta.Mensaje);
            return RedirigirALista();
        }
        return View(respuesta.Datos);
    }

    // POST: Plazas/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var respuesta = await api.DeleteAsync<bool>($"api/Plazas/{id}");
        if (VerificarSesion(respuesta) is { } redireccion) return redireccion;

        if (respuesta.Exito)
            MensajeExito("Plaza eliminada correctamente.");
        else
            MensajeError(respuesta.Mensaje);

        return RedirigirALista();
    }

    // GET: Plazas/Postulaciones/5  -> candidatos postulados a la plaza
    public async Task<IActionResult> Postulaciones(int id)
    {
        var plaza = await api.GetAsync<PlazaDto>($"api/Plazas/{id}");
        if (!plaza.Exito || plaza.Datos == null)
        {
            MensajeError(plaza.Mensaje);
            return RedirigirALista();
        }

        var respuesta = await api.GetAsync<List<PostulacionDto>>($"api/Postulaciones/plaza/{id}");
        if (VerificarSesion(respuesta) is { } redireccion) return redireccion;
        if (!respuesta.Exito)
        {
            MensajeError(respuesta.Mensaje);
            return RedirigirALista();
        }

        ViewBag.Plaza = plaza.Datos;
        return View(respuesta.Datos ?? []);
    }

    private IActionResult RedirigirALista()
    {
        return User.IsInRole(Roles.Administrador) ? RedirectToAction(nameof(Todas)) : RedirectToAction(nameof(MisPlazas));
    }
}
