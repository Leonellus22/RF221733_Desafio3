using AutoMapper;
using BolsaTrabajo.BL.Interfaces;
using BolsaTrabajo.Common;
using BolsaTrabajo.Entities.Dtos;
using BolsaTrabajo.Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BolsaTrabajo.BL.Services;

// Gestión de usuarios y roles mediante ASP.NET Core Identity
public class UsuarioService(
    UserManager<Usuario> userManager,
    RoleManager<IdentityRole> roleManager,
    IMapper mapper
    ) : IUsuarioService
{
    public async Task<List<UsuarioDto>> GetUsuariosAsync()
    {
        var usuarios = await userManager.Users.OrderBy(u => u.Rol).ThenBy(u => u.NombreCompleto).ToListAsync();
        return mapper.Map<List<Usuario>, List<UsuarioDto>>(usuarios);
    }

    public async Task<ResultadoOperacion<UsuarioDto>> GetUsuarioByIdAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return ResultadoOperacion<UsuarioDto>.Fallo(TipoResultado.NoAutorizado, "Debe iniciar sesión.");

        var usuario = await userManager.FindByIdAsync(id);
        return usuario == null
            ? ResultadoOperacion<UsuarioDto>.Fallo(TipoResultado.NoEncontrado, "El usuario no existe.")
            : ResultadoOperacion<UsuarioDto>.Correcto(mapper.Map<Usuario, UsuarioDto>(usuario));
    }

    // Registro público: siempre se asigna el rol Candidato
    public async Task<ResultadoOperacion<UsuarioDto>> RegistrarCandidatoAsync(RegistroDto dto)
    {
        return await CrearAsync(dto, Roles.Candidato);
    }

    // Creación de usuarios por parte del administrador (cualquier rol)
    public async Task<ResultadoOperacion<UsuarioDto>> CrearUsuarioAsync(CrearUsuarioDto dto)
    {
        if (!Roles.Todos.Contains(dto.Rol))
            return ResultadoOperacion<UsuarioDto>.Fallo(TipoResultado.Validacion, "El rol indicado no es válido.");

        return await CrearAsync(dto, dto.Rol);
    }

    public async Task<ResultadoOperacion<UsuarioDto>> ActualizarPerfilAsync(string? id, ActualizarPerfilDto dto)
    {
        if (string.IsNullOrWhiteSpace(id))
            return ResultadoOperacion<UsuarioDto>.Fallo(TipoResultado.NoAutorizado, "Debe iniciar sesión.");

        var usuario = await userManager.FindByIdAsync(id);
        if (usuario == null)
            return ResultadoOperacion<UsuarioDto>.Fallo(TipoResultado.NoEncontrado, "El usuario no existe.");

        usuario.NombreCompleto = dto.NombreCompleto.Trim();
        usuario.PhoneNumber = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();

        var resultado = await userManager.UpdateAsync(usuario);
        return resultado.Succeeded
            ? ResultadoOperacion<UsuarioDto>.Correcto(mapper.Map<Usuario, UsuarioDto>(usuario), "Perfil actualizado correctamente.")
            : ResultadoOperacion<UsuarioDto>.Fallo(TipoResultado.Validacion, UnirErrores(resultado));
    }

    public async Task<ResultadoOperacion<UsuarioDto>> CambiarRolAsync(string id, string rol, string? idUsuarioActual)
    {
        if (!Roles.Todos.Contains(rol))
            return ResultadoOperacion<UsuarioDto>.Fallo(TipoResultado.Validacion, "El rol indicado no es válido.");

        if (id == idUsuarioActual)
            return ResultadoOperacion<UsuarioDto>.Fallo(TipoResultado.Validacion, "No puede cambiar su propio rol.");

        var usuario = await userManager.FindByIdAsync(id);
        if (usuario == null)
            return ResultadoOperacion<UsuarioDto>.Fallo(TipoResultado.NoEncontrado, "El usuario no existe.");

        // Se reemplazan los roles actuales por el nuevo rol
        var rolesActuales = await userManager.GetRolesAsync(usuario);
        await userManager.RemoveFromRolesAsync(usuario, rolesActuales);
        await userManager.AddToRoleAsync(usuario, rol);

        usuario.Rol = rol;
        await userManager.UpdateAsync(usuario);

        return ResultadoOperacion<UsuarioDto>.Correcto(mapper.Map<Usuario, UsuarioDto>(usuario), "Rol actualizado correctamente.");
    }

    public async Task<ResultadoOperacion<bool>> EliminarUsuarioAsync(string id, string? idUsuarioActual)
    {
        if (id == idUsuarioActual)
            return ResultadoOperacion<bool>.Fallo(TipoResultado.Validacion, "No puede eliminar su propio usuario.");

        var usuario = await userManager.FindByIdAsync(id);
        if (usuario == null)
            return ResultadoOperacion<bool>.Fallo(TipoResultado.NoEncontrado, "El usuario no existe.");

        try
        {
            var resultado = await userManager.DeleteAsync(usuario);
            return resultado.Succeeded
                ? ResultadoOperacion<bool>.Correcto(true, "Usuario eliminado correctamente.")
                : ResultadoOperacion<bool>.Fallo(TipoResultado.Validacion, UnirErrores(resultado));
        }
        catch (DbUpdateException)
        {
            // El usuario tiene plazas publicadas o postulaciones asociadas
            return ResultadoOperacion<bool>.Fallo(TipoResultado.Conflicto, "No se puede eliminar el usuario porque tiene plazas o postulaciones registradas.");
        }
    }

    public async Task<List<RolDto>> GetRolesAsync()
    {
        var roles = await roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
        var lista = new List<RolDto>();
        foreach (var rol in roles)
        {
            var usuarios = await userManager.GetUsersInRoleAsync(rol.Name!);
            lista.Add(new RolDto { Nombre = rol.Name!, TotalUsuarios = usuarios.Count });
        }
        return lista;
    }

    private async Task<ResultadoOperacion<UsuarioDto>> CrearAsync(RegistroDto dto, string rol)
    {
        if (await userManager.FindByEmailAsync(dto.Email) != null)
            return ResultadoOperacion<UsuarioDto>.Fallo(TipoResultado.Conflicto, "Ya existe un usuario registrado con ese correo.");

        var usuario = new Usuario
        {
            UserName = dto.Email.Trim(),
            Email = dto.Email.Trim(),
            NombreCompleto = dto.NombreCompleto.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim(),
            Rol = rol
        };

        var resultado = await userManager.CreateAsync(usuario, dto.Password);
        if (!resultado.Succeeded)
            return ResultadoOperacion<UsuarioDto>.Fallo(TipoResultado.Validacion, UnirErrores(resultado));

        await userManager.AddToRoleAsync(usuario, rol);
        return ResultadoOperacion<UsuarioDto>.Correcto(mapper.Map<Usuario, UsuarioDto>(usuario), "Usuario registrado correctamente.");
    }

    private static string UnirErrores(IdentityResult resultado)
    {
        return string.Join(" ", resultado.Errors.Select(e => e.Description));
    }
}
