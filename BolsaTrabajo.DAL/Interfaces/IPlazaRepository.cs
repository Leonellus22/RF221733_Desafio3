using BolsaTrabajo.Entities.Models;

namespace BolsaTrabajo.DAL.Interfaces;

public interface IPlazaRepository
{
    public Task<List<Plaza>> GetPlazasAsync();
    public Task<List<Plaza>> GetPlazasDisponiblesAsync(DateTime fecha);
    public Task<List<Plaza>> GetPlazasPorAgenteAsync(string idAgente);
    public Task<Plaza?> GetPlazaByIdAsync(int id);
    public Task<int> InsertPlazaAsync(Plaza plaza);
    public Task<Plaza?> UpdatePlazaAsync(Plaza plaza);
    public Task<bool> DeletePlazaAsync(int id);
    public Task<int> CountPlazasAsync();
}
