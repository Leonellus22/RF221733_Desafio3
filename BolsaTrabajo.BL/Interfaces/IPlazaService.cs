using BolsaTrabajo.Common;
using BolsaTrabajo.Entities.Dtos;
using BolsaTrabajo.Entities.Enums;

namespace BolsaTrabajo.BL.Interfaces;

public interface IPlazaService
{
    public Task<List<PlazaDto>> GetAllPlazasAsync();
    public Task<List<PlazaDto>> GetPlazasDisponiblesAsync();
    public Task<List<PlazaDto>> GetPlazasPorAgenteAsync(string idAgente);
    public Task<PlazaDto?> GetPlazaByIdAsync(int id);
    public Task<ResultadoOperacion<PlazaDto>> CrearPlazaAsync(PlazaDto plazaDto, string? idAgente);
    public Task<ResultadoOperacion<PlazaDto>> ActualizarPlazaAsync(int id, PlazaDto plazaDto, string? idUsuario, bool esAdministrador);
    public Task<ResultadoOperacion<bool>> EliminarPlazaAsync(int id, string? idUsuario, bool esAdministrador);
    public Task<ResultadoOperacion<PlazaDto>> CambiarEstadoAsync(int id, EstadoPlaza estado, string? idUsuario, bool esAdministrador);
}
