using BolsaTrabajo.Common;
using BolsaTrabajo.Entities.Dtos;
using BolsaTrabajo.Entities.Enums;

namespace BolsaTrabajo.BL.Interfaces;

public interface IPostulacionService
{
    public Task<List<PostulacionDto>> GetAllPostulacionesAsync(EstadoPostulacion? estado = null);
    public Task<List<PostulacionDto>> GetPostulacionesPorCandidatoAsync(string idCandidato);
    public Task<ResultadoOperacion<List<PostulacionDto>>> GetPostulacionesPorPlazaAsync(int idPlaza, string? idUsuario, bool esAdministrador);
    public Task<ResultadoOperacion<PostulacionDto>> PostularAsync(int idPlaza, string? idCandidato);
    public Task<ResultadoOperacion<PostulacionDto>> EvaluarAsync(int idPostulacion, EvaluarPostulacionDto dto, string? idUsuario, bool esAdministrador);
}
