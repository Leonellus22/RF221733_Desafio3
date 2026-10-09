using System.Security.Claims;
using BolsaTrabajo.Entities.Dtos;
using BolsaTrabajo.MVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolsaTrabajo.MVC.Controllers;

// Inicio de sesión, registro y perfil del usuario
public class CuentaController(ApiClient api) : BaseController
{
    // GET: Cuenta/Login
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    // POST: Cuenta/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        if (!ModelState.IsValid)
            return View(dto);

        // 1. Se obtiene el token desde el endpoint de Identity
        var token = await api.LoginAsync(dto);
        if (!token.Exito || token.Datos == null)
        {
            ModelState.AddModelError(string.Empty, token.Mensaje);
            return View(dto);
        }

        // 2. Se consultan los datos del usuario (nombre y rol) con el token recibido
        var perfil = await api.GetAsync<UsuarioDto>("api/Usuarios/perfil", token.Datos.AccessToken);
        if (!perfil.Exito || perfil.Datos == null)
        {
            ModelState.AddModelError(string.Empty, perfil.Mensaje);
            return View(dto);
        }

        // 3. Se crea la cookie de autenticación con los datos y el token
        await IniciarSesionAsync(perfil.Datos, token.Datos.AccessToken);
        MensajeExito($"Bienvenido(a), {perfil.Datos.NombreCompleto}.");

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return perfil.Datos.Rol switch
        {
            Common.Roles.Administrador => RedirectToAction("Index", "Usuarios"),
            Common.Roles.Agente => RedirectToAction("MisPlazas", "Plazas"),
            _ => RedirectToAction("Index", "Home")
        };
    }

    // GET: Cuenta/Registro
    public IActionResult Registro()
    {
        return View();
    }

    // POST: Cuenta/Registro  -> registro de candidatos
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registro(RegistroDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var respuesta = await api.PostAsync<UsuarioDto>("api/Usuarios/registro", dto);
        if (!respuesta.Exito)
        {
            ModelState.AddModelError(string.Empty, respuesta.Mensaje);
            return View(dto);
        }

        MensajeExito("Registro completado. Inicie sesión para completar su hoja de vida.");
        return RedirectToAction(nameof(Login));
    }

    // POST: Cuenta/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    // GET: Cuenta/SesionExpirada  -> se llama cuando la API rechaza el token
    public async Task<IActionResult> SesionExpirada()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        MensajeError("Su sesión expiró. Inicie sesión nuevamente.");
        return RedirectToAction(nameof(Login));
    }

    // GET: Cuenta/AccesoDenegado
    public IActionResult AccesoDenegado()
    {
        return View();
    }

    // GET: Cuenta/Perfil
    [Authorize]
    public async Task<IActionResult> Perfil()
    {
        var respuesta = await api.GetAsync<UsuarioDto>("api/Usuarios/perfil");
        if (VerificarSesion(respuesta) is { } redireccion) return redireccion;
        if (!respuesta.Exito || respuesta.Datos == null)
        {
            MensajeError(respuesta.Mensaje);
            return RedirectToAction("Index", "Home");
        }

        ViewBag.Usuario = respuesta.Datos;
        return View(new ActualizarPerfilDto { NombreCompleto = respuesta.Datos.NombreCompleto, Telefono = respuesta.Datos.Telefono });
    }

    // POST: Cuenta/Perfil
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Perfil(ActualizarPerfilDto dto)
    {
        if (ModelState.IsValid)
        {
            var respuesta = await api.PutAsync<UsuarioDto>("api/Usuarios/perfil", dto);
            if (VerificarSesion(respuesta) is { } redireccion) return redireccion;
            if (respuesta.Exito && respuesta.Datos != null)
            {
                // Se renueva la cookie para que el nuevo nombre aparezca en el menú
                await IniciarSesionAsync(respuesta.Datos, User.FindFirst(TokenHandler.ClaimToken)!.Value);
                MensajeExito("Perfil actualizado correctamente.");
                return RedirectToAction(nameof(Perfil));
            }
            ModelState.AddModelError(string.Empty, respuesta.Mensaje);
        }

        var perfil = await api.GetAsync<UsuarioDto>("api/Usuarios/perfil");
        ViewBag.Usuario = perfil.Datos;
        return View(dto);
    }

    private async Task IniciarSesionAsync(UsuarioDto usuario, string token)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id),
            new(ClaimTypes.Name, usuario.NombreCompleto),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Rol),
            new(TokenHandler.ClaimToken, token)
        };
        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidad));
    }
}
