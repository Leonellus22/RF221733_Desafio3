using BolsaTrabajo.Entities.Models;

namespace BolsaTrabajo.DAL.Interfaces;

public interface IHojaDeVidaRepository
{
    public Task<HojaDeVida?> GetHojaDeVidaByIdAsync(int id);
    public Task<HojaDeVida?> GetHojaDeVidaPorUsuarioAsync(string idUsuario);
    public Task<int> InsertHojaDeVidaAsync(HojaDeVida hojaDeVida);
    public Task<HojaDeVida?> UpdateHojaDeVidaAsync(HojaDeVida hojaDeVida);
}
