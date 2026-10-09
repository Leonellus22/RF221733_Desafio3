using BolsaTrabajo.Common;
using BolsaTrabajo.Entities.Dtos;
using BolsaTrabajo.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolsaTrabajo.MVC.Controllers;

// Gestión de usuarios y roles (solo administradores)
[Authorize(Roles = Roles.Administrador)]
public class UsuariosController(ApiClient api) : BaseController
{
    // GET: Usuarios
    public async Task<IActionResult> Index(string? rol)
    {
        var respuesta = await api.GetAsync<List<UsuarioDto>>("api/Usuarios");
        if (VerificarSesion(respuesta) is { } redireccion) return redireccion;
        if (!respuesta.Exito) ViewBag.Error = respuesta.Mensaje;

        var roles = await api.GetAsync<List<RolDto>>("api/Usuarios/roles");
        ViewBag.Roles = roles.Datos ?? [];
        ViewBag.Rol = rol;

        var usuarios = respuesta.Datos ?? [];
        if (!string.IsNullOrEmpty(rol))
            usuarios = usuarios.Where(u => u.Rol == rol).ToList();

        return View(usuarios);
    }

    // GET: Usuarios/Create
    public IActionResult Create()
    {
        return View(new CrearUsuarioDto { Rol = Roles.Agente });
    }

    // POST: Usuarios/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrearUsuarioDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var respuesta = await api.PostAsync<UsuarioDto>("api/Usuarios", dto);
        if (VerificarSesion(respuesta) is { } redireccion) return redireccion;
        if (!respuesta.Exito)
        {
            ModelState.AddModelError(string.Empty, respuesta.Mensaje);
            return View(dto);
        }

        MensajeExito($"Usuario {respuesta.Datos!.Email} creado con el rol {respuesta.Datos.Rol}.");
        return RedirectToAction(nameof(Index));
    }

    // POST: Usuarios/CambiarRol/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarRol(string id, string rol)
    {
        var respuesta = await api.PutAsync<UsuarioDto>($"api/Usuarios/{id}/rol", new CambiarRolDto { Rol = rol });
        if (VerificarSesion(respuesta) is { } redireccion) return redireccion;

        if (respuesta.Exito)
            MensajeExito($"{respuesta.Datos!.NombreCompleto} ahora tiene el rol {respuesta.Datos.Rol}.");
        else
            MensajeError(respuesta.Mensaje);

        return RedirectToAction(nameof(Index));
    }

    // GET: Usuarios/Delete/{id}
    public async Task<IActionResult> Delete(string id)
    {
        var respuesta = await api.GetAsync<UsuarioDto>($"api/Usuarios/{id}");
        if (VerificarSesion(respuesta) is { } redireccion) return redireccion;
        if (!respuesta.Exito || respuesta.Datos == null)
        {
            MensajeError(respuesta.Mensaje);
            return RedirectToAction(nameof(Index));
        }
        return View(respuesta.Datos);
    }

    // POST: Usuarios/Delete/{id}
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var respuesta = await api.DeleteAsync<bool>($"api/Usuarios/{id}");
        if (VerificarSesion(respuesta) is { } redireccion) return redireccion;

        if (respuesta.Exito)
            MensajeExito("Usuario eliminado correctamente.");
        else
            MensajeError(respuesta.Mensaje);

        return RedirectToAction(nameof(Index));
    }
}
