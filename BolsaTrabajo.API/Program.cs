using System.Text.Json.Serialization;
using BolsaTrabajo.BL.Services;
using BolsaTrabajo.Common;
using BolsaTrabajo.DAL.Data;
using BolsaTrabajo.Entities.Models;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---> REGISTRO DEL CONTEXTO DE LA BASE DE DATOS (SQL Server)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Configuración de Swagger/OpenAPI (https://aka.ms/aspnetcore/swashbuckle)
builder.Services.AddEndpointsApiExplorer();

// Swagger UI con soporte para enviar el token Bearer
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Bolsa de Trabajo - API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        In = ParameterLocation.Header,
        Description = "Ingrese el accessToken obtenido en /api/auth/login"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

builder.Services.Configure<ArchivoSettings>(builder.Configuration.GetSection("ArchivoSettings"));

// INICIO -> AUTENTICACION Y AUTORIZACION
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<Usuario>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>()                       // Habilita los roles (Administrador, Agente, Candidato)
    .AddErrorDescriber<IdentityErrorDescriberEs>()  // Mensajes de error en español
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Duración del token Bearer que devuelve /api/auth/login
builder.Services.Configure<BearerTokenOptions>(IdentityConstants.BearerScheme, options =>
    options.BearerTokenExpiration = TimeSpan.FromHours(2));
// FIN -> AUTENTICACION Y AUTORIZACION

builder.Services.AddServiceConnector(); //<-- Registra AutoMapper, servicios (BL) y repositorios (DAL)

var app = builder.Build();

// Aplica las migraciones y crea los datos iniciales (roles, usuarios y plazas de ejemplo)
await DbInitializer.InicializarAsync(app.Services);

// Configuración del pipeline de peticiones HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// Endpoints de Identity: /api/auth/login, /api/auth/refresh, etc.
app.MapGroup("/api/auth").WithTags("Auth").MapIdentityApi<Usuario>();

app.Run();

// Necesario para poder referenciar la clase Program desde el proyecto de pruebas
public partial class Program { }
