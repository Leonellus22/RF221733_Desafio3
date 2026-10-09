using System.Security.Claims;
using AutoMapper;
using BolsaTrabajo.BL.AutoMapper;
using BolsaTrabajo.Common;
using BolsaTrabajo.DAL.Data;
using BolsaTrabajo.Entities.Enums;
using BolsaTrabajo.Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BolsaTrabajo.Tests;

public class Setup
{
    public const string IdAgente = "agente-1";
    public const string IdOtroAgente = "agente-2";
    public const string IdCandidato = "candidato-1";

    // Crea un contexto con una base de datos en memoria distinta para cada prueba
    protected ApplicationDbContext ConstruirContext(string nombreDB)
    {
        var opciones = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(nombreDB).Options;

        return new ApplicationDbContext(opciones);
    }

    // Configura AutoMapper con el mismo perfil que utiliza la API
    protected IMapper ConfigurarAutoMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(_ => { }, typeof(AutoMapperProfile));
        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    // Asigna al controlador un usuario autenticado con el rol indicado
    protected static void AutenticarUsuario(ControllerBase controller, string idUsuario, string rol)
    {
        var identidad = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, idUsuario),
            new Claim(ClaimTypes.Role, rol)
        ], "Pruebas");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identidad) }
        };
    }

    // Simula una petición sin usuario autenticado
    protected static void SinUsuario(ControllerBase controller)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // Inserta los usuarios base (dos agentes y un candidato)
    protected static void AgregarUsuarios(ApplicationDbContext context)
    {
        context.Users.AddRange(
            new Usuario { Id = IdAgente, UserName = "agente@test.com", Email = "agente@test.com", NombreCompleto = "Agente Uno", Rol = Roles.Agente },
            new Usuario { Id = IdOtroAgente, UserName = "agente2@test.com", Email = "agente2@test.com", NombreCompleto = "Agente Dos", Rol = Roles.Agente },
            new Usuario { Id = IdCandidato, UserName = "candidato@test.com", Email = "candidato@test.com", NombreCompleto = "Candidato Uno", Rol = Roles.Candidato });
        context.SaveChanges();
    }

    // Inserta una plaza y devuelve su id
    protected static int AgregarPlaza(ApplicationDbContext context, EstadoPlaza estado = EstadoPlaza.Abierta)
    {
        var plaza = new Plaza
        {
            Titulo = "Técnico en Informática",
            Descripcion = "Soporte técnico a usuarios de la institución.",
            Institucion = "Ministerio de Hacienda",
            FechaPublicacion = DateTime.Today,
            FechaCierre = DateTime.Today.AddDays(10),
            Estado = estado,
            IdAgente = IdAgente
        };
        context.Plazas.Add(plaza);
        context.SaveChanges();
        return plaza.IdPlaza;
    }

    // Inserta la hoja de vida del candidato (requisito para postularse)
    protected static void AgregarHojaDeVida(ApplicationDbContext context, string idUsuario = IdCandidato)
    {
        context.HojasDeVida.Add(new HojaDeVida
        {
            IdUsuario = idUsuario,
            FormacionAcademica = "Ingeniería en Sistemas Informáticos",
            ExperienciaLaboral = "Dos años como técnico de soporte",
            Competencias = "C#, SQL Server, trabajo en equipo"
        });
        context.SaveChanges();
    }
}
