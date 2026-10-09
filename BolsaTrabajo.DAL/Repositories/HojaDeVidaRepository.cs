using BolsaTrabajo.DAL.Data;
using BolsaTrabajo.DAL.Interfaces;
using BolsaTrabajo.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace BolsaTrabajo.DAL.Repositories;

// Acceso a datos de las hojas de vida mediante Entity Framework Core
public class HojaDeVidaRepository(ApplicationDbContext context) : IHojaDeVidaRepository
{
    public async Task<HojaDeVida?> GetHojaDeVidaByIdAsync(int id)
    {
        return await context.HojasDeVida
            .Include(h => h.Usuario)
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.IdCV == id);
    }

    public async Task<HojaDeVida?> GetHojaDeVidaPorUsuarioAsync(string idUsuario)
    {
        return await context.HojasDeVida
            .Include(h => h.Usuario)
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.IdUsuario == idUsuario);
    }

    public async Task<int> InsertHojaDeVidaAsync(HojaDeVida hojaDeVida)
    {
        context.HojasDeVida.Add(hojaDeVida);
        await context.SaveChangesAsync();
        return hojaDeVida.IdCV;
    }

    public async Task<HojaDeVida?> UpdateHojaDeVidaAsync(HojaDeVida hojaDeVida)
    {
        var entidad = await context.HojasDeVida.FindAsync(hojaDeVida.IdCV);
        if (entidad == null)
            return null;

        entidad.FormacionAcademica = hojaDeVida.FormacionAcademica;
        entidad.ExperienciaLaboral = hojaDeVida.ExperienciaLaboral;
        entidad.Competencias = hojaDeVida.Competencias;
        entidad.ArchivoCV = hojaDeVida.ArchivoCV;

        await context.SaveChangesAsync();
        return entidad;
    }
}
