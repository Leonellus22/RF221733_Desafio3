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
public class UsuariosController(IUsuarioService service) : ControllerBase
{
    // POST: api/Usuarios/registro  -> registro público de candidatos
    [HttpPost("registro")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registro(RegistroDto dto)
    {
        var resultado = await service.RegistrarCandidatoAsync(dto);
        return resultado.Exito
            ? CreatedAtAction(nameof(GetPerfil), null, resultado.Datos)
            : this.ToActionResult(resultado);
    }

    // GET: api/Usuarios/perfil  -> datos del usuario autenticado
    [HttpGet("perfil")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPerfil()
    {
        var resultado = await service.GetUsuarioByIdAsync(this.ObtenerIdUsuario());
        return this.ToActionResult(resultado);
    }

    // PUT: api/Usuarios/perfil
    [HttpPut("perfil")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PutPerfil(ActualizarPerfilDto dto)
    {
        var resultado = await service.ActualizarPerfilAsync(this.ObtenerIdUsuario(), dto);
        return this.ToActionResult(resultado);
    }

    // GET: api/Usuarios
    [HttpGet]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType(typeof(IEnumerable<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        return Ok(await service.GetUsuariosAsync());
    }

    // GET: api/Usuarios/{id}
    [HttpGet("{id}")]
    [Authorize(Roles = Roles.AgenteOAdministrador)]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string id)
    {
        return this.ToActionResult(await service.GetUsuarioByIdAsync(id));
    }

    // POST: api/Usuarios  -> el administrador crea usuarios con cualquier rol
    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post(CrearUsuarioDto dto)
    {
        var resultado = await service.CrearUsuarioAsync(dto);
        return resultado.Exito
            ? CreatedAtAction(nameof(Get), new { id = resultado.Datos!.Id }, resultado.Datos)
            : this.ToActionResult(resultado);
    }

    // PUT: api/Usuarios/{id}/rol
    [HttpPut("{id}/rol")]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PutRol(string id, CambiarRolDto dto)
    {
        return this.ToActionResult(await service.CambiarRolAsync(id, dto.Rol, this.ObtenerIdUsuario()));
    }

    // DELETE: api/Usuarios/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(string id)
    {
        return this.ToActionResult(await service.EliminarUsuarioAsync(id, this.ObtenerIdUsuario()));
    }

    // GET: api/Usuarios/roles
    [HttpGet("roles")]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType(typeof(IEnumerable<RolDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles()
    {
        return Ok(await service.GetRolesAsync());
    }
}
