using System.Diagnostics;
using BolsaTrabajo.Entities.Dtos;
using BolsaTrabajo.MVC.Models;
using BolsaTrabajo.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace BolsaTrabajo.MVC.Controllers;

public class HomeController(ApiClient api) : BaseController
{
    // GET: Home  -> listado público de plazas disponibles
    public async Task<IActionResult> Index(string? buscar)
    {
        var respuesta = await api.GetAsync<List<PlazaDto>>("api/Plazas/disponibles");
        if (!respuesta.Exito)
            ViewBag.Error = respuesta.Mensaje;

        var plazas = respuesta.Datos ?? [];
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            plazas = plazas.Where(p => p.Titulo.Contains(buscar, StringComparison.OrdinalIgnoreCase)
                                    || p.Institucion.Contains(buscar, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        ViewBag.Buscar = buscar;
        return View(plazas);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
