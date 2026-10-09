using BolsaTrabajo.Common;
using BolsaTrabajo.Entities.Enums;
using BolsaTrabajo.Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BolsaTrabajo.DAL.Data;

// Aplica las migraciones y crea los roles, usuarios y plazas iniciales
public static class DbInitializer
{
    public static async Task InicializarAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Creación/actualización automática de la base de datos mediante migraciones
        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync();
        }

        // ================================================
        // ROLES
        // ================================================
        foreach (var rol in Roles.Todos)
        {
            if (!await roleManager.RoleExistsAsync(rol))
            {
                await roleManager.CreateAsync(new IdentityRole(rol));
            }
        }

        // ================================================
        // USUARIOS INICIALES
        // ================================================
        await CrearUsuarioAsync(userManager, "admin@bolsatrabajo.gob.sv", "Administrador General", "Admin123*", Roles.Administrador);
        var agente = await CrearUsuarioAsync(userManager, "agente@bolsatrabajo.gob.sv", "Ana Martínez", "Agente123*", Roles.Agente);
        await CrearUsuarioAsync(userManager, "candidato@bolsatrabajo.gob.sv", "Carlos López", "Candidato123*", Roles.Candidato);

        // ================================================
        // PLAZAS DE EJEMPLO
        // ================================================
        if (!await context.Plazas.AnyAsync())
        {
            var hoy = DateTime.Today;
            context.Plazas.AddRange(
                new Plaza
                {
                    Titulo = "Técnico en Soporte Informático",
                    Descripcion = "Brindar soporte técnico a usuarios, mantenimiento preventivo y correctivo de equipos informáticos.",
                    Institucion = "Ministerio de Hacienda",
                    FechaPublicacion = hoy,
                    FechaCierre = hoy.AddDays(30),
                    Estado = EstadoPlaza.Abierta,
                    IdAgente = agente.Id
                },
                new Plaza
                {
                    Titulo = "Desarrollador de Software .NET",
                    Descripcion = "Desarrollo y mantenimiento de sistemas institucionales con ASP.NET Core y SQL Server.",
                    Institucion = "Ministerio de Educación",
                    FechaPublicacion = hoy,
                    FechaCierre = hoy.AddDays(20),
                    Estado = EstadoPlaza.Abierta,
                    IdAgente = agente.Id
                },
                new Plaza
                {
                    Titulo = "Asistente Administrativo",
                    Descripcion = "Apoyo en la gestión documental, atención al público y control de correspondencia institucional.",
                    Institucion = "Ministerio de Salud",
                    FechaPublicacion = hoy,
                    FechaCierre = hoy.AddDays(15),
                    Estado = EstadoPlaza.Abierta,
                    IdAgente = agente.Id
                });

            await context.SaveChangesAsync();
        }
    }

    private static async Task<Usuario> CrearUsuarioAsync(UserManager<Usuario> userManager, string email, string nombre, string password, string rol)
    {
        var usuario = await userManager.FindByEmailAsync(email);
        if (usuario != null)
            return usuario;

        usuario = new Usuario
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            NombreCompleto = nombre,
            Rol = rol
        };

        await userManager.CreateAsync(usuario, password);
        await userManager.AddToRoleAsync(usuario, rol);
        return usuario;
    }
}
