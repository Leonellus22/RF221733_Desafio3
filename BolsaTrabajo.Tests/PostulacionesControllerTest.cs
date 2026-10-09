using BolsaTrabajo.API.Controllers;
using BolsaTrabajo.BL.Services;
using BolsaTrabajo.Common;
using BolsaTrabajo.DAL.Data;
using BolsaTrabajo.DAL.Repositories;
using BolsaTrabajo.Entities.Dtos;
using BolsaTrabajo.Entities.Enums;
using Microsoft.AspNetCore.Mvc;

namespace BolsaTrabajo.Tests;

public class PostulacionesControllerTest : Setup
{
    private PostulacionService ConstruirService(ApplicationDbContext context)
    {
        return new PostulacionService(new PostulacionRepository(context), new PlazaRepository(context), new HojaDeVidaRepository(context), ConfigurarAutoMapper());
    }

    private PostulacionesController ConstruirController(ApplicationDbContext context)
    {
        return new PostulacionesController(ConstruirService(context));
    }

    [Fact]
    public async Task Post_SinUsuarioAutenticado_RetornaUnauthorized()
    {
        // Arrange
        var nombreDB = Guid.NewGuid().ToString();
        var context = ConstruirContext(nombreDB);
        AgregarUsuarios(context);
        var idPlaza = AgregarPlaza(context);
        var controller = ConstruirController(context);
        SinUsuario(controller);

        // Act
        var respuesta = await controller.Post(new CrearPostulacionDto { IdPlaza = idPlaza });

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(respuesta);
        Assert.Equal(0, ConstruirContext(nombreDB).Postulaciones.Count());
    }

    [Fact]
    public async Task PostularAsync_SinIdDeCandidato_GeneraError()
    {
        // Arrange
        var context = ConstruirContext(Guid.NewGuid().ToString());
        AgregarUsuarios(context);
        var idPlaza = AgregarPlaza(context);
        var service = ConstruirService(context);

        // Act
        var resultado = await service.PostularAsync(idPlaza, null);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal(TipoResultado.NoAutorizado, resultado.Tipo);
    }

    [Fact]
    public async Task Post_CandidatoSePostulaDosVecesALaMismaPlaza_RetornaConflict()
    {
        // Arrange
        var nombreDB = Guid.NewGuid().ToString();
        var context = ConstruirContext(nombreDB);
        AgregarUsuarios(context);
        AgregarHojaDeVida(context);
        var idPlaza = AgregarPlaza(context);
        var controller = ConstruirController(context);
        AutenticarUsuario(controller, IdCandidato, Roles.Candidato);
        var dto = new CrearPostulacionDto { IdPlaza = idPlaza };

        // Act
        var primera = await controller.Post(dto);
        var segunda = await controller.Post(dto);

        // Assert
        Assert.IsType<CreatedAtActionResult>(primera);
        Assert.IsType<ConflictObjectResult>(segunda);
        Assert.Equal(1, ConstruirContext(nombreDB).Postulaciones.Count());
    }

    [Fact]
    public async Task Post_PlazaCerrada_RetornaBadRequest()
    {
        // Arrange
        var context = ConstruirContext(Guid.NewGuid().ToString());
        AgregarUsuarios(context);
        AgregarHojaDeVida(context);
        var idPlaza = AgregarPlaza(context, EstadoPlaza.Cerrada);
        var controller = ConstruirController(context);
        AutenticarUsuario(controller, IdCandidato, Roles.Candidato);

        // Act
        var respuesta = await controller.Post(new CrearPostulacionDto { IdPlaza = idPlaza });

        // Assert
        Assert.IsType<BadRequestObjectResult>(respuesta);
    }

    [Fact]
    public async Task Post_CandidatoSinHojaDeVida_RetornaBadRequest()
    {
        // Arrange
        var context = ConstruirContext(Guid.NewGuid().ToString());
        AgregarUsuarios(context);
        var idPlaza = AgregarPlaza(context);
        var controller = ConstruirController(context);
        AutenticarUsuario(controller, IdCandidato, Roles.Candidato);

        // Act
        var respuesta = await controller.Post(new CrearPostulacionDto { IdPlaza = idPlaza });

        // Assert
        Assert.IsType<BadRequestObjectResult>(respuesta);
        Assert.Equal(0, context.Postulaciones.Count());
    }

    [Fact]
    public async Task PutEstado_AgenteDeLaPlazaApruebaPostulacion_CambiaElEstado()
    {
        // Arrange
        var context = ConstruirContext(Guid.NewGuid().ToString());
        AgregarUsuarios(context);
        AgregarHojaDeVida(context);
        var idPlaza = AgregarPlaza(context);
        var service = ConstruirService(context);
        var postulacion = await service.PostularAsync(idPlaza, IdCandidato);
        var controller = ConstruirController(context);
        AutenticarUsuario(controller, IdAgente, Roles.Agente);

        // Act
        var respuesta = await controller.PutEstado(postulacion.Datos!.IdPostulacion,
            new EvaluarPostulacionDto { Estado = EstadoPostulacion.Aprobado, Observaciones = "Cumple el perfil" });

        // Assert
        var resultado = Assert.IsType<OkObjectResult>(respuesta);
        var dto = Assert.IsType<PostulacionDto>(resultado.Value);
        Assert.Equal(EstadoPostulacion.Aprobado, dto.Estado);
    }

    [Fact]
    public async Task PutEstado_AgenteQueNoPublicoLaPlaza_RetornaForbidden()
    {
        // Arrange
        var context = ConstruirContext(Guid.NewGuid().ToString());
        AgregarUsuarios(context);
        AgregarHojaDeVida(context);
        var idPlaza = AgregarPlaza(context);
        var postulacion = await ConstruirService(context).PostularAsync(idPlaza, IdCandidato);
        var controller = ConstruirController(context);
        AutenticarUsuario(controller, IdOtroAgente, Roles.Agente);

        // Act
        var respuesta = await controller.PutEstado(postulacion.Datos!.IdPostulacion,
            new EvaluarPostulacionDto { Estado = EstadoPostulacion.Rechazado });

        // Assert
        var resultado = Assert.IsType<ObjectResult>(respuesta);
        Assert.Equal(403, resultado.StatusCode);
    }
}
