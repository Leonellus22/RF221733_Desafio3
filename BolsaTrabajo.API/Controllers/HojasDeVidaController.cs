using BolsaTrabajo.API.Extensions;
using BolsaTrabajo.BL.Interfaces;
using BolsaTrabajo.Common;
using BolsaTrabajo.Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolsaTrabajo.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class HojasDeVidaController(IHojaDeVidaService service, IWebHostEnvironment environment) : ControllerBase
{
    // GET: api/HojasDeVida/mia  -> hoja de vida del candidato autenticado
    [HttpGet("mia")]
    [Authorize(Roles = Roles.Candidato)]
    [ProducesResponseType(typeof(HojaDeVidaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetMia()
    {
        var result = await service.GetHojaDeVidaPorUsuarioAsync(this.ObtenerIdUsuario()!);
        return result != null ? Ok(result) : NoContent();
    }

    // PUT: api/HojasDeVida/mia  -> crea o actualiza la hoja de vida
    [HttpPut("mia")]
    [Authorize(Roles = Roles.Candidato)]
    [ProducesResponseType(typeof(HojaDeVidaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PutMia(HojaDeVidaDto dto)
    {
        return this.ToActionResult(await service.GuardarHojaDeVidaAsync(this.ObtenerIdUsuario(), dto));
    }

    // POST: api/HojasDeVida/mia/archivo  -> carga del CV en PDF (multipart/form-data)
    [HttpPost("mia/archivo")]
    [Authorize(Roles = Roles.Candidato)]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [ProducesResponseType(typeof(HojaDeVidaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostArchivo(IFormFile archivo)
    {
        if (archivo == null)
            return BadRequest(new { mensaje = "Debe seleccionar un archivo." });

        await using var stream = archivo.OpenReadStream();
        var resultado = await service.GuardarArchivoAsync(this.ObtenerIdUsuario(), stream, archivo.FileName, archivo.Length, environment.ContentRootPath);
        return this.ToActionResult(resultado);
    }

    // GET: api/HojasDeVida/usuario/{idUsuario}  -> el agente o administrador consulta el CV de un candidato
    [HttpGet("usuario/{idUsuario}")]
    [Authorize(Roles = Roles.AgenteOAdministrador)]
    [ProducesResponseType(typeof(HojaDeVidaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPorUsuario(string idUsuario)
    {
        var result = await service.GetHojaDeVidaPorUsuarioAsync(idUsuario);
        return result != null ? Ok(result) : NotFound(new { mensaje = "El candidato no tiene hoja de vida registrada." });
    }

    // GET: api/HojasDeVida/5/archivo  -> descarga del PDF
    [HttpGet("{id:int}/archivo")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetArchivo(int id)
    {
        var puedeVerTodos = User.IsInRole(Roles.Agente) || User.IsInRole(Roles.Administrador);
        var resultado = await service.ObtenerRutaArchivoAsync(id, this.ObtenerIdUsuario(), puedeVerTodos, environment.ContentRootPath);
        if (!resultado.Exito)
            return this.ToActionResult(resultado);

        return PhysicalFile(resultado.Datos!, "application/pdf", Path.GetFileName(resultado.Datos));
    }
}
