using BolsaTrabajo.Entities.Models;

namespace BolsaTrabajo.DAL.Interfaces;

public interface IPostulacionRepository
{
    public Task<List<Postulacion>> GetPostulacionesAsync();
    public Task<List<Postulacion>> GetPostulacionesPorCandidatoAsync(string idCandidato);
    public Task<List<Postulacion>> GetPostulacionesPorPlazaAsync(int idPlaza);
    public Task<Postulacion?> GetPostulacionByIdAsync(int id);
    public Task<bool> ExistePostulacionAsync(int idPlaza, string idCandidato);
    public Task<int> InsertPostulacionAsync(Postulacion postulacion);
    public Task<Postulacion?> UpdatePostulacionAsync(Postulacion postulacion);
    public Task<int> RechazarPendientesAsync(int idPlaza);
}
