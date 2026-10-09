using BolsaTrabajo.DAL.Data;
using BolsaTrabajo.DAL.Interfaces;
using BolsaTrabajo.Entities.Enums;
using BolsaTrabajo.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace BolsaTrabajo.DAL.Repositories;

// Acceso a datos de las plazas mediante Entity Framework Core
public class PlazaRepository(ApplicationDbContext context) : IPlazaRepository
{
    // Consulta base que incluye el agente y las postulaciones (para el conteo)
    private IQueryable<Plaza> ConsultaBase()
    {
        return context.Plazas
            .Include(p => p.Agente)
            .Include(p => p.Postulaciones)
            .AsNoTracking();
    }

    public async Task<List<Plaza>> GetPlazasAsync()
    {
        return await ConsultaBase()
            .OrderByDescending(p => p.FechaPublicacion)
            .ToListAsync();
    }

    // Plazas abiertas cuyo periodo de publicación está vigente en la fecha indicada
    public async Task<List<Plaza>> GetPlazasDisponiblesAsync(DateTime fecha)
    {
        var dia = fecha.Date;
        return await ConsultaBase()
            .Where(p => p.Estado == EstadoPlaza.Abierta && p.FechaPublicacion <= dia && p.FechaCierre >= dia)
            .OrderBy(p => p.FechaCierre)
            .ToListAsync();
    }

    public async Task<List<Plaza>> GetPlazasPorAgenteAsync(string idAgente)
    {
        return await ConsultaBase()
            .Where(p => p.IdAgente == idAgente)
            .OrderByDescending(p => p.FechaPublicacion)
            .ToListAsync();
    }

    public async Task<Plaza?> GetPlazaByIdAsync(int id)
    {
        return await ConsultaBase().FirstOrDefaultAsync(p => p.IdPlaza == id);
    }

    public async Task<int> InsertPlazaAsync(Plaza plaza)
    {
        context.Plazas.Add(plaza);
        await context.SaveChangesAsync();
        return plaza.IdPlaza;
    }

    public async Task<Plaza?> UpdatePlazaAsync(Plaza plaza)
    {
        var entidad = await context.Plazas.FindAsync(plaza.IdPlaza);
        if (entidad == null)
            return null;

        entidad.Titulo = plaza.Titulo;
        entidad.Descripcion = plaza.Descripcion;
        entidad.FechaPublicacion = plaza.FechaPublicacion;
        entidad.FechaCierre = plaza.FechaCierre;
        entidad.Institucion = plaza.Institucion;
        entidad.Estado = plaza.Estado;

        await context.SaveChangesAsync();
        return entidad;
    }

    public async Task<bool> DeletePlazaAsync(int id)
    {
        var entidad = await context.Plazas.FindAsync(id);
        if (entidad == null)
            return false;

        context.Plazas.Remove(entidad);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<int> CountPlazasAsync()
    {
        return await context.Plazas.CountAsync();
    }
}
