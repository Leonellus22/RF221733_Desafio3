using BolsaTrabajo.DAL.Interfaces;
using BolsaTrabajo.DAL.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace BolsaTrabajo.DAL.Services;

public static class ServiceCollectionExtensions
{
    // Registra los repositorios de la capa de datos
    public static IServiceCollection AddRepositoryConnector(this IServiceCollection services)
    {
        services.AddScoped<IPlazaRepository, PlazaRepository>();
        services.AddScoped<IPostulacionRepository, PostulacionRepository>();
        services.AddScoped<IHojaDeVidaRepository, HojaDeVidaRepository>();
        return services;
    }
}
