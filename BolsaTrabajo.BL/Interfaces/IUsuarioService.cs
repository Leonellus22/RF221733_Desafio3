using BolsaTrabajo.Common;
using BolsaTrabajo.Entities.Dtos;

namespace BolsaTrabajo.BL.Interfaces;

public interface IUsuarioService
{
    public Task<List<UsuarioDto>> GetUsuariosAsync();
    public Task<ResultadoOperacion<UsuarioDto>> GetUsuarioByIdAsync(string? id);
    public Task<ResultadoOperacion<UsuarioDto>> RegistrarCandidatoAsync(RegistroDto dto);
    public Task<ResultadoOperacion<UsuarioDto>> CrearUsuarioAsync(CrearUsuarioDto dto);
    public Task<ResultadoOperacion<UsuarioDto>> ActualizarPerfilAsync(string? id, ActualizarPerfilDto dto);
    public Task<ResultadoOperacion<UsuarioDto>> CambiarRolAsync(string id, string rol, string? idUsuarioActual);
    public Task<ResultadoOperacion<bool>> EliminarUsuarioAsync(string id, string? idUsuarioActual);
    public Task<List<RolDto>> GetRolesAsync();
}
