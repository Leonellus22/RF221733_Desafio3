using BolsaTrabajo.Common;
using BolsaTrabajo.Entities.Dtos;
using BolsaTrabajo.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolsaTrabajo.MVC.Controllers;

// Gestión de la hoja de vida del candidato
[Authorize]
public class HojaDeVidaController(ApiClient api) : BaseController
{
    // GET: HojaDeVida
    [Authorize(Roles = Roles.Candidato)]
    public async Task<IActionResult> Index()
    {
        var respuesta = await api.GetAsync<HojaDeVidaDto>("api/HojasDeVida/mia");
        if (VerificarSesion(respuesta) is { } redireccion) return redireccion;
        if (!respuesta.Exito) ViewBag.Error = respuesta.Mensaje;

        // Si la API devuelve 204 el candidato todavía no ha registrado su hoja de vida
        return View(respuesta.Datos ?? new HojaDeVidaDto());
    }

    // POST: HojaDeVida  -> guarda los datos de la hoja de vida
    [Authorize(Roles = Roles.Candidato)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(HojaDeVidaDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var respuesta = await api.PutAsync<HojaDeVidaDto>("api/HojasDeVida/mia", dto);
        if (VerificarSesion(respuesta) is { } redireccion) return redireccion;
        if (!respuesta.Exito)
        {
            ModelState.AddModelError(string.Empty, respuesta.Mensaje);
            return View(dto);
        }

        MensajeExito("Hoja de vida guardada correctamente.");
        return RedirectToAction(nameof(Index));
    }

    // POST: HojaDeVida/SubirArchivo  -> carga del CV en PDF
    [Authorize(Roles = Roles.Candidato)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> SubirArchivo(IFormFile? archivo)
    {
        if (archivo == null || archivo.Length == 0)
        {
            MensajeError("Debe seleccionar un archivo PDF.");
            return RedirectToAction(nameof(Index));
        }

        if (!string.Equals(Path.GetExtension(archivo.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            MensajeError("Solo se permiten archivos PDF.");
            return RedirectToAction(nameof(Index));
        }

        var respuesta = await api.PostArchivoAsync<HojaDeVidaDto>("api/HojasDeVida/mia/archivo", archivo, "archivo");
        if (VerificarSesion(respuesta) is { } redireccion) return redireccion;

        if (respuesta.Exito)
            MensajeExito("Archivo CV cargado correctamente.");
        else
            MensajeError(respuesta.Mensaje);

        return RedirectToAction(nameof(Index));
    }

    // GET: HojaDeVida/Archivo/5  -> muestra el PDF del CV
    public async Task<IActionResult> Archivo(int id)
    {
        var archivo = await api.GetArchivoAsync($"api/HojasDeVida/{id}/archivo");
        if (!archivo.Exito)
        {
            MensajeError(archivo.Mensaje);
            return User.IsInRole(Roles.Candidato) ? RedirectToAction(nameof(Index)) : RedirectToAction("Index", "Home");
        }
        return File(archivo.Contenido, archivo.TipoContenido);
    }

    // GET: HojaDeVida/Candidato/{idUsuario}  -> el agente o administrador revisa la hoja de vida de un postulante
    [Authorize(Roles = Roles.AgenteOAdministrador)]
    public async Task<IActionResult> Candidato(string id, int? idPlaza)
    {
        var respuesta = await api.GetAsync<HojaDeVidaDto>($"api/HojasDeVida/usuario/{id}");
        if (VerificarSesion(respuesta) is { } redireccion) return redireccion;
        if (!respuesta.Exito || respuesta.Datos == null)
        {
            MensajeError(respuesta.Mensaje);
            return idPlaza.HasValue ? RedirectToAction("Postulaciones", "Plazas", new { id = idPlaza }) : RedirectToAction("Index", "Home");
        }

        ViewBag.IdPlaza = idPlaza;
        return View(respuesta.Datos);
    }
}
