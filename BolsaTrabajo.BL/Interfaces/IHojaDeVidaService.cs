using BolsaTrabajo.Common;
using BolsaTrabajo.Entities.Dtos;

namespace BolsaTrabajo.BL.Interfaces;

public interface IHojaDeVidaService
{
    public Task<HojaDeVidaDto?> GetHojaDeVidaPorUsuarioAsync(string idUsuario);
    public Task<HojaDeVidaDto?> GetHojaDeVidaByIdAsync(int id);
    public Task<ResultadoOperacion<HojaDeVidaDto>> GuardarHojaDeVidaAsync(string? idUsuario, HojaDeVidaDto dto);
    public Task<ResultadoOperacion<HojaDeVidaDto>> GuardarArchivoAsync(string? idUsuario, Stream contenido, string nombreArchivo, long tamano, string rutaRaiz);
    public Task<ResultadoOperacion<string>> ObtenerRutaArchivoAsync(int idCV, string? idUsuario, bool puedeVerTodos, string rutaRaiz);
}
