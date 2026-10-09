using BolsaTrabajo.API.Extensions;
using BolsaTrabajo.BL.Interfaces;
using BolsaTrabajo.Common;
using BolsaTrabajo.Entities.Dtos;
using BolsaTrabajo.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolsaTrabajo.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PostulacionesController(IPostulacionService service) : ControllerBase
{
    // GET: api/Postulaciones?estado=EnRevision  -> monitoreo de todas las postulaciones
    [HttpGet]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType(typeof(IEnumerable<PostulacionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] EstadoPostulacion? estado)
    {
        return Ok(await service.GetAllPostulacionesAsync(estado));
    }

    // GET: api/Postulaciones/mis-postulaciones
    [HttpGet("mis-postulaciones")]
    [Authorize(Roles = Roles.Candidato)]
    [ProducesResponseType(typeof(IEnumerable<PostulacionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMisPostulaciones()
    {
        return Ok(await service.GetPostulacionesPorCandidatoAsync(this.ObtenerIdUsuario()!));
    }

    // GET: api/Postulaciones/plaza/5  -> postulaciones de una plaza (agente dueño o administrador)
    [HttpGet("plaza/{idPlaza:int}")]
    [Authorize(Roles = Roles.AgenteOAdministrador)]
    [ProducesResponseType(typeof(IEnumerable<PostulacionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPorPlaza(int idPlaza)
    {
        var resultado = await service.GetPostulacionesPorPlazaAsync(idPlaza, this.ObtenerIdUsuario(), this.EsAdministrador());
        return this.ToActionResult(resultado);
    }

    // POST: api/Postulaciones  -> el candidato autenticado se postula a una plaza
    [HttpPost]
    [Authorize(Roles = Roles.Candidato)]
    [ProducesResponseType(typeof(PostulacionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Post(CrearPostulacionDto dto)
    {
        var resultado = await service.PostularAsync(dto.IdPlaza, this.ObtenerIdUsuario());
        return resultado.Exito
            ? CreatedAtAction(nameof(GetMisPostulaciones), null, resultado.Datos)
            : this.ToActionResult(resultado);
    }

    // PUT: api/Postulaciones/5/estado  -> evaluación (En revisión, Aprobado, Rechazado)
    [HttpPut("{id:int}/estado")]
    [Authorize(Roles = Roles.AgenteOAdministrador)]
    [ProducesResponseType(typeof(PostulacionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PutEstado(int id, EvaluarPostulacionDto dto)
    {
        var resultado = await service.EvaluarAsync(id, dto, this.ObtenerIdUsuario(), this.EsAdministrador());
        return this.ToActionResult(resultado);
    }
}
