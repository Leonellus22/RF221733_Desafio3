using BolsaTrabajo.DAL.Data;
using BolsaTrabajo.DAL.Interfaces;
using BolsaTrabajo.Entities.Enums;
using BolsaTrabajo.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace BolsaTrabajo.DAL.Repositories;

// Acceso a datos de las postulaciones mediante Entity Framework Core
public class PostulacionRepository(ApplicationDbContext context) : IPostulacionRepository
{
    private IQueryable<Postulacion> ConsultaBase()
    {
        return context.Postulaciones
            .Include(p => p.Plaza)
            .Include(p => p.Candidato)
            .AsNoTracking();
    }

    public async Task<List<Postulacion>> GetPostulacionesAsync()
    {
        return await ConsultaBase()
            .OrderByDescending(p => p.FechaPostulacion)
            .ToListAsync();
    }

    public async Task<List<Postulacion>> GetPostulacionesPorCandidatoAsync(string idCandidato)
    {
        return await ConsultaBase()
            .Where(p => p.IdCandidato == idCandidato)
            .OrderByDescending(p => p.FechaPostulacion)
            .ToListAsync();
    }

    public async Task<List<Postulacion>> GetPostulacionesPorPlazaAsync(int idPlaza)
    {
        return await ConsultaBase()
            .Where(p => p.IdPlaza == idPlaza)
            .OrderBy(p => p.FechaPostulacion)
            .ToListAsync();
    }

    public async Task<Postulacion?> GetPostulacionByIdAsync(int id)
    {
        return await ConsultaBase().FirstOrDefaultAsync(p => p.IdPostulacion == id);
    }

    public async Task<bool> ExistePostulacionAsync(int idPlaza, string idCandidato)
    {
        return await context.Postulaciones.AnyAsync(p => p.IdPlaza == idPlaza && p.IdCandidato == idCandidato);
    }

    public async Task<int> InsertPostulacionAsync(Postulacion postulacion)
    {
        context.Postulaciones.Add(postulacion);
        await context.SaveChangesAsync();
        return postulacion.IdPostulacion;
    }

    public async Task<Postulacion?> UpdatePostulacionAsync(Postulacion postulacion)
    {
        var entidad = await context.Postulaciones.FindAsync(postulacion.IdPostulacion);
        if (entidad == null)
            return null;

        entidad.Estado = postulacion.Estado;
        entidad.Observaciones = postulacion.Observaciones;

        await context.SaveChangesAsync();
        return entidad;
    }

    // Marca como rechazadas las postulaciones que siguen en revisión (se usa al cerrar una plaza)
    public async Task<int> RechazarPendientesAsync(int idPlaza)
    {
        var pendientes = await context.Postulaciones
            .Where(p => p.IdPlaza == idPlaza && p.Estado == EstadoPostulacion.EnRevision)
            .ToListAsync();

        foreach (var postulacion in pendientes)
        {
            postulacion.Estado = EstadoPostulacion.Rechazado;
            postulacion.Observaciones ??= "Plaza cerrada sin selección.";
        }

        await context.SaveChangesAsync();
        return pendientes.Count;
    }
}
