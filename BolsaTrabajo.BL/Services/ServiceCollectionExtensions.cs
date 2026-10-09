using BolsaTrabajo.BL.AutoMapper;
using BolsaTrabajo.BL.Interfaces;
using BolsaTrabajo.DAL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BolsaTrabajo.BL.Services;

public static class ServiceCollectionExtensions
{
    // Registra AutoMapper, los servicios de negocio y los repositorios
    public static IServiceCollection AddServiceConnector(this IServiceCollection services)
    {
        services.AddAutoMapper(_ => { }, typeof(AutoMapperProfile));
        services.AddScoped<IPlazaService, PlazaService>();
        services.AddScoped<IPostulacionService, PostulacionService>();
        services.AddScoped<IHojaDeVidaService, HojaDeVidaService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddRepositoryConnector();
        return services;
    }
}
