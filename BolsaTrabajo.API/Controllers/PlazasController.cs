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
public class PlazasController(IPlazaService service) : ControllerBase
{
    // GET: api/Plazas/disponibles  -> plazas abiertas y vigentes (acceso público)
    [HttpGet("disponibles")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<PlazaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDisponibles()
    {
        return Ok(await service.GetPlazasDisponiblesAsync());
    }

    // GET: api/Plazas  -> todas las plazas (monitoreo del administrador)
    [HttpGet]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType(typeof(IEnumerable<PlazaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        return Ok(await service.GetAllPlazasAsync());
    }

    // GET: api/Plazas/mis-plazas  -> plazas publicadas por el agente autenticado
    [HttpGet("mis-plazas")]
    [Authorize(Roles = Roles.Agente)]
    [ProducesResponseType(typeof(IEnumerable<PlazaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMisPlazas()
    {
        return Ok(await service.GetPlazasPorAgenteAsync(this.ObtenerIdUsuario()!));
    }

    // GET: api/Plazas/5
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PlazaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id)
    {
        var result = await service.GetPlazaByIdAsync(id);
        return result != null ? Ok(result) : NotFound(new { mensaje = "La plaza no existe." });
    }

    // POST: api/Plazas
    [HttpPost]
    [Authorize(Roles = Roles.Agente)]
    [ProducesResponseType(typeof(PlazaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post(PlazaDto dto)
    {
        var resultado = await service.CrearPlazaAsync(dto, this.ObtenerIdUsuario());
        return resultado.Exito
            ? CreatedAtAction(nameof(Get), new { id = resultado.Datos!.IdPlaza }, resultado.Datos)
            : this.ToActionResult(resultado);
    }

    // PUT: api/Plazas/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.AgenteOAdministrador)]
    [ProducesResponseType(typeof(PlazaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Put(int id, PlazaDto dto)
    {
        if (dto.IdPlaza != 0 && dto.IdPlaza != id)
            return BadRequest(new { mensaje = "El id de la plaza no coincide." });

        var resultado = await service.ActualizarPlazaAsync(id, dto, this.ObtenerIdUsuario(), this.EsAdministrador());
        return this.ToActionResult(resultado);
    }

    // PUT: api/Plazas/5/estado  -> cambia el estado del proceso (Abierta, En evaluación, Cerrada)
    [HttpPut("{id:int}/estado")]
    [Authorize(Roles = Roles.AgenteOAdministrador)]
    [ProducesResponseType(typeof(PlazaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PutEstado(int id, CambiarEstadoPlazaDto dto)
    {
        var resultado = await service.CambiarEstadoAsync(id, dto.Estado, this.ObtenerIdUsuario(), this.EsAdministrador());
        return this.ToActionResult(resultado);
    }

    // DELETE: api/Plazas/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AgenteOAdministrador)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id)
    {
        var resultado = await service.EliminarPlazaAsync(id, this.ObtenerIdUsuario(), this.EsAdministrador());
        return this.ToActionResult(resultado);
    }
}
