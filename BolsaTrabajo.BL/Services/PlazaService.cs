using AutoMapper;
using BolsaTrabajo.BL.Interfaces;
using BolsaTrabajo.Common;
using BolsaTrabajo.DAL.Interfaces;
using BolsaTrabajo.Entities.Dtos;
using BolsaTrabajo.Entities.Enums;
using BolsaTrabajo.Entities.Models;

namespace BolsaTrabajo.BL.Services;

// Lógica de negocio de las plazas
public class PlazaService(
    IPlazaRepository repository,
    IPostulacionRepository postulacionRepository,
    IMapper mapper
    ) : IPlazaService
{
    public async Task<List<PlazaDto>> GetAllPlazasAsync()
    {
        var result = await repository.GetPlazasAsync();
        return mapper.Map<List<Plaza>, List<PlazaDto>>(result);
    }

    public async Task<List<PlazaDto>> GetPlazasDisponiblesAsync()
    {
        var result = await repository.GetPlazasDisponiblesAsync(DateTime.Today);
        return mapper.Map<List<Plaza>, List<PlazaDto>>(result);
    }

    public async Task<List<PlazaDto>> GetPlazasPorAgenteAsync(string idAgente)
    {
        var result = await repository.GetPlazasPorAgenteAsync(idAgente);
        return mapper.Map<List<Plaza>, List<PlazaDto>>(result);
    }

    public async Task<PlazaDto?> GetPlazaByIdAsync(int id)
    {
        var result = await repository.GetPlazaByIdAsync(id);
        return result == null ? null : mapper.Map<Plaza, PlazaDto>(result);
    }

    public async Task<ResultadoOperacion<PlazaDto>> CrearPlazaAsync(PlazaDto plazaDto, string? idAgente)
    {
        if (string.IsNullOrWhiteSpace(idAgente))
            return ResultadoOperacion<PlazaDto>.Fallo(TipoResultado.NoAutorizado, "Debe iniciar sesión como agente para publicar plazas.");

        var errorDatos = ValidarDatos(plazaDto);
        if (errorDatos != null)
            return ResultadoOperacion<PlazaDto>.Fallo(TipoResultado.Validacion, errorDatos);

        // Al crear, la fecha de publicación no puede estar en el pasado
        var errorFechas = ValidarFechas(plazaDto.FechaPublicacion, plazaDto.FechaCierre, validarPublicacionPasada: true);
        if (errorFechas != null)
            return ResultadoOperacion<PlazaDto>.Fallo(TipoResultado.Validacion, errorFechas);

        var entity = mapper.Map<PlazaDto, Plaza>(plazaDto);
        entity.IdPlaza = 0;
        entity.IdAgente = idAgente;
        entity.Estado = EstadoPlaza.Abierta;
        entity.FechaPublicacion = plazaDto.FechaPublicacion.Date;
        entity.FechaCierre = plazaDto.FechaCierre.Date;

        var id = await repository.InsertPlazaAsync(entity);
        var creada = await repository.GetPlazaByIdAsync(id);
        return ResultadoOperacion<PlazaDto>.Correcto(mapper.Map<Plaza, PlazaDto>(creada ?? entity), "Plaza publicada correctamente.");
    }

    public async Task<ResultadoOperacion<PlazaDto>> ActualizarPlazaAsync(int id, PlazaDto plazaDto, string? idUsuario, bool esAdministrador)
    {
        var plaza = await repository.GetPlazaByIdAsync(id);
        var errorAcceso = ValidarAcceso(plaza, idUsuario, esAdministrador);
        if (errorAcceso != null)
            return ResultadoOperacion<PlazaDto>.Fallo(errorAcceso.Value.tipo, errorAcceso.Value.mensaje);

        if (plaza!.Estado == EstadoPlaza.Cerrada)
            return ResultadoOperacion<PlazaDto>.Fallo(TipoResultado.Validacion, "No se puede editar una plaza cerrada.");

        var errorDatos = ValidarDatos(plazaDto);
        if (errorDatos != null)
            return ResultadoOperacion<PlazaDto>.Fallo(TipoResultado.Validacion, errorDatos);

        // Solo se valida que la nueva fecha de publicación no sea pasada si fue modificada
        var publicacionModificada = plaza.FechaPublicacion.Date != plazaDto.FechaPublicacion.Date;
        var errorFechas = ValidarFechas(plazaDto.FechaPublicacion, plazaDto.FechaCierre, publicacionModificada);
        if (errorFechas != null)
            return ResultadoOperacion<PlazaDto>.Fallo(TipoResultado.Validacion, errorFechas);

        plaza.Titulo = plazaDto.Titulo.Trim();
        plaza.Descripcion = plazaDto.Descripcion.Trim();
        plaza.Institucion = plazaDto.Institucion.Trim();
        plaza.FechaPublicacion = plazaDto.FechaPublicacion.Date;
        plaza.FechaCierre = plazaDto.FechaCierre.Date;

        await repository.UpdatePlazaAsync(plaza);
        var actualizada = await repository.GetPlazaByIdAsync(id);
        return ResultadoOperacion<PlazaDto>.Correcto(mapper.Map<Plaza, PlazaDto>(actualizada!), "Plaza actualizada correctamente.");
    }

    public async Task<ResultadoOperacion<bool>> EliminarPlazaAsync(int id, string? idUsuario, bool esAdministrador)
    {
        var plaza = await repository.GetPlazaByIdAsync(id);
        var errorAcceso = ValidarAcceso(plaza, idUsuario, esAdministrador);
        if (errorAcceso != null)
            return ResultadoOperacion<bool>.Fallo(errorAcceso.Value.tipo, errorAcceso.Value.mensaje);

        // Para no perder el historial del proceso, una plaza con postulaciones solo puede cerrarse
        if (plaza!.Postulaciones.Count > 0)
            return ResultadoOperacion<bool>.Fallo(TipoResultado.Conflicto, "La plaza tiene postulaciones registradas; cámbiela al estado Cerrada en lugar de eliminarla.");

        var eliminado = await repository.DeletePlazaAsync(id);
        return eliminado
            ? ResultadoOperacion<bool>.Correcto(true, "Plaza eliminada correctamente.")
            : ResultadoOperacion<bool>.Fallo(TipoResultado.NoEncontrado, "La plaza no existe.");
    }

    public async Task<ResultadoOperacion<PlazaDto>> CambiarEstadoAsync(int id, EstadoPlaza estado, string? idUsuario, bool esAdministrador)
    {
        if (!Enum.IsDefined(estado))
            return ResultadoOperacion<PlazaDto>.Fallo(TipoResultado.Validacion, "El estado indicado no es válido.");

        var plaza = await repository.GetPlazaByIdAsync(id);
        var errorAcceso = ValidarAcceso(plaza, idUsuario, esAdministrador);
        if (errorAcceso != null)
            return ResultadoOperacion<PlazaDto>.Fallo(errorAcceso.Value.tipo, errorAcceso.Value.mensaje);

        if (plaza!.Estado == EstadoPlaza.Cerrada)
            return ResultadoOperacion<PlazaDto>.Fallo(TipoResultado.Validacion, "La plaza ya está cerrada y su proceso de selección finalizó.");

        plaza.Estado = estado;
        await repository.UpdatePlazaAsync(plaza);

        // Al cerrar la plaza, las postulaciones que siguen en revisión se rechazan
        if (estado == EstadoPlaza.Cerrada)
        {
            await postulacionRepository.RechazarPendientesAsync(id);
        }

        var actualizada = await repository.GetPlazaByIdAsync(id);
        return ResultadoOperacion<PlazaDto>.Correcto(mapper.Map<Plaza, PlazaDto>(actualizada!), $"La plaza ahora está {estado.ObtenerNombre()}.");
    }

    // Reglas de negocio para las fechas de publicación y cierre
    public static string? ValidarFechas(DateTime fechaPublicacion, DateTime fechaCierre, bool validarPublicacionPasada)
    {
        if (fechaPublicacion == default || fechaCierre == default)
            return "Las fechas de publicación y cierre son obligatorias.";

        if (validarPublicacionPasada && fechaPublicacion.Date < DateTime.Today)
            return "La fecha de publicación no puede ser anterior a la fecha actual.";

        if (fechaCierre.Date <= fechaPublicacion.Date)
            return "La fecha de cierre debe ser posterior a la fecha de publicación.";

        return null;
    }

    private static string? ValidarDatos(PlazaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Titulo))
            return "El título es obligatorio.";
        if (string.IsNullOrWhiteSpace(dto.Descripcion))
            return "La descripción es obligatoria.";
        if (string.IsNullOrWhiteSpace(dto.Institucion))
            return "La institución es obligatoria.";
        return null;
    }

    // Verifica que la plaza exista y que el usuario sea el agente que la publicó o un administrador
    private static (TipoResultado tipo, string mensaje)? ValidarAcceso(Plaza? plaza, string? idUsuario, bool esAdministrador)
    {
        if (string.IsNullOrWhiteSpace(idUsuario))
            return (TipoResultado.NoAutorizado, "Debe iniciar sesión.");
        if (plaza == null)
            return (TipoResultado.NoEncontrado, "La plaza no existe.");
        if (!esAdministrador && plaza.IdAgente != idUsuario)
            return (TipoResultado.Prohibido, "Solo el agente que publicó la plaza puede modificarla.");
        return null;
    }
}
