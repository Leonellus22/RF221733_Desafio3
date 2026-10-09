using System.ComponentModel.DataAnnotations;
using BolsaTrabajo.API.Controllers;
using BolsaTrabajo.BL.Services;
using BolsaTrabajo.Common;
using BolsaTrabajo.DAL.Data;
using BolsaTrabajo.DAL.Repositories;
using BolsaTrabajo.Entities.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BolsaTrabajo.Tests;

public class PlazasControllerTest : Setup
{
    private PlazasController ConstruirController(ApplicationDbContext context)
    {
        var service = new PlazaService(new PlazaRepository(context), new PostulacionRepository(context), ConfigurarAutoMapper());
        return new PlazasController(service);
    }

    private static PlazaDto PlazaValida() => new()
    {
        Titulo = "Analista de Recursos Humanos",
        Descripcion = "Gestión de procesos de reclutamiento del personal.",
        Institucion = "Ministerio de Trabajo",
        FechaPublicacion = DateTime.Today,
        FechaCierre = DateTime.Today.AddDays(15)
    };

    [Fact]
    public async Task Post_PlazaValida_AumentaElConteoDeRegistros()
    {
        // Arrange
        var nombreDB = Guid.NewGuid().ToString();
        var context = ConstruirContext(nombreDB);
        AgregarUsuarios(context);
        var controller = ConstruirController(context);
        AutenticarUsuario(controller, IdAgente, Roles.Agente);
        var conteoInicial = context.Plazas.Count();

        // Act
        var respuesta = await controller.Post(PlazaValida());

        // Assert
        var resultado = Assert.IsType<CreatedAtActionResult>(respuesta);
        var plaza = Assert.IsType<PlazaDto>(resultado.Value);
        Assert.Equal(IdAgente, plaza.IdAgente);

        var context2 = ConstruirContext(nombreDB);
        Assert.Equal(conteoInicial + 1, context2.Plazas.Count());
    }

    [Fact]
    public async Task Post_FechaCierreAnteriorALaPublicacion_RetornaBadRequestYNoGuarda()
    {
        // Arrange
        var nombreDB = Guid.NewGuid().ToString();
        var context = ConstruirContext(nombreDB);
        AgregarUsuarios(context);
        var controller = ConstruirController(context);
        AutenticarUsuario(controller, IdAgente, Roles.Agente);
        var plaza = PlazaValida();
        plaza.FechaCierre = plaza.FechaPublicacion.AddDays(-1);

        // Act
        var respuesta = await controller.Post(plaza);

        // Assert
        Assert.IsType<BadRequestObjectResult>(respuesta);
        Assert.Equal(0, ConstruirContext(nombreDB).Plazas.Count());
    }

    [Fact]
    public async Task Post_FechaPublicacionEnElPasado_RetornaBadRequest()
    {
        // Arrange
        var context = ConstruirContext(Guid.NewGuid().ToString());
        AgregarUsuarios(context);
        var controller = ConstruirController(context);
        AutenticarUsuario(controller, IdAgente, Roles.Agente);
        var plaza = PlazaValida();
        plaza.FechaPublicacion = DateTime.Today.AddDays(-3);

        // Act
        var respuesta = await controller.Post(plaza);

        // Assert
        Assert.IsType<BadRequestObjectResult>(respuesta);
        Assert.Equal(0, context.Plazas.Count());
    }

    [Theory]
    [InlineData(0, 1, true)]     // Cierre un día después de la publicación: válido
    [InlineData(0, 30, true)]    // Cierre un mes después: válido
    [InlineData(0, 0, false)]    // Cierre el mismo día de la publicación: inválido
    [InlineData(5, 2, false)]    // Cierre antes de la publicación: inválido
    [InlineData(-1, 10, false)]  // Publicación en el pasado: inválido
    public void ValidarFechas_VerificaPublicacionYCierre(int diasPublicacion, int diasCierre, bool esValido)
    {
        // Arrange
        var publicacion = DateTime.Today.AddDays(diasPublicacion);
        var cierre = DateTime.Today.AddDays(diasCierre);

        // Act
        var error = PlazaService.ValidarFechas(publicacion, cierre, validarPublicacionPasada: true);

        // Assert
        Assert.Equal(esValido, error == null);
    }

    [Fact]
    public void PlazaDto_FechaCierreInvalida_GeneraErrorDeValidacion()
    {
        // Arrange
        var plaza = PlazaValida();
        plaza.FechaCierre = plaza.FechaPublicacion;
        var errores = new List<ValidationResult>();

        // Act
        var esValido = Validator.TryValidateObject(plaza, new ValidationContext(plaza), errores, validateAllProperties: true);

        // Assert
        Assert.False(esValido);
        Assert.Contains(errores, e => e.MemberNames.Contains(nameof(PlazaDto.FechaCierre)));
    }

    [Fact]
    public async Task Put_AgenteQueNoPublicoLaPlaza_RetornaForbidden()
    {
        // Arrange
        var context = ConstruirContext(Guid.NewGuid().ToString());
        AgregarUsuarios(context);
        var idPlaza = AgregarPlaza(context);
        var controller = ConstruirController(context);
        AutenticarUsuario(controller, IdOtroAgente, Roles.Agente);

        // Act
        var respuesta = await controller.Put(idPlaza, PlazaValida());

        // Assert
        var resultado = Assert.IsType<ObjectResult>(respuesta);
        Assert.Equal(403, resultado.StatusCode);
    }

    [Fact]
    public async Task GetDisponibles_SoloDevuelvePlazasAbiertas()
    {
        // Arrange
        var context = ConstruirContext(Guid.NewGuid().ToString());
        AgregarUsuarios(context);
        AgregarPlaza(context);
        AgregarPlaza(context, Entities.Enums.EstadoPlaza.Cerrada);
        var controller = ConstruirController(context);

        // Act
        var respuesta = await controller.GetDisponibles();

        // Assert
        var resultado = Assert.IsType<OkObjectResult>(respuesta);
        var plazas = Assert.IsType<List<PlazaDto>>(resultado.Value);
        Assert.Single(plazas);
    }
}
