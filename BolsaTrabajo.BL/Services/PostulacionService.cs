using AutoMapper;
using BolsaTrabajo.BL.Interfaces;
using BolsaTrabajo.Common;
using BolsaTrabajo.DAL.Interfaces;
using BolsaTrabajo.Entities.Dtos;
using BolsaTrabajo.Entities.Enums;
using BolsaTrabajo.Entities.Models;

namespace BolsaTrabajo.BL.Services;

// Lógica de negocio del proceso de postulación y evaluación
public class PostulacionService(
    IPostulacionRepository repository,
    IPlazaRepository plazaRepository,
    IHojaDeVidaRepository hojaDeVidaRepository,
    IMapper mapper
    ) : IPostulacionService
{
    public async Task<List<PostulacionDto>> GetAllPostulacionesAsync(EstadoPostulacion? estado = null)
    {
        var result = await repository.GetPostulacionesAsync();
        if (estado.HasValue)
            result = result.Where(p => p.Estado == estado.Value).ToList();
        return mapper.Map<List<Postulacion>, List<PostulacionDto>>(result);
    }

    public async Task<List<PostulacionDto>> GetPostulacionesPorCandidatoAsync(string idCandidato)
    {
        var result = await repository.GetPostulacionesPorCandidatoAsync(idCandidato);
        return mapper.Map<List<Postulacion>, List<PostulacionDto>>(result);
    }

    public async Task<ResultadoOperacion<List<PostulacionDto>>> GetPostulacionesPorPlazaAsync(int idPlaza, string? idUsuario, bool esAdministrador)
    {
        var plaza = await plazaRepository.GetPlazaByIdAsync(idPlaza);
        if (plaza == null)
            return ResultadoOperacion<List<PostulacionDto>>.Fallo(TipoResultado.NoEncontrado, "La plaza no existe.");
        if (!esAdministrador && plaza.IdAgente != idUsuario)
            return ResultadoOperacion<List<PostulacionDto>>.Fallo(TipoResultado.Prohibido, "Solo el agente que publicó la plaza puede ver sus postulaciones.");

        var result = await repository.GetPostulacionesPorPlazaAsync(idPlaza);
        return ResultadoOperacion<List<PostulacionDto>>.Correcto(mapper.Map<List<Postulacion>, List<PostulacionDto>>(result));
    }

    public async Task<ResultadoOperacion<PostulacionDto>> PostularAsync(int idPlaza, string? idCandidato)
    {
        // 1. El candidato debe estar autenticado
        if (string.IsNullOrWhiteSpace(idCandidato))
            return ResultadoOperacion<PostulacionDto>.Fallo(TipoResultado.NoAutorizado, "Debe iniciar sesión para postularse a una plaza.");

        // 2. La plaza debe existir
        var plaza = await plazaRepository.GetPlazaByIdAsync(idPlaza);
        if (plaza == null)
            return ResultadoOperacion<PostulacionDto>.Fallo(TipoResultado.NoEncontrado, "La plaza no existe.");

        // 3. La plaza debe estar abierta y dentro del periodo de publicación
        var errorPlaza = ValidarPlazaDisponible(plaza, DateTime.Today);
        if (errorPlaza != null)
            return ResultadoOperacion<PostulacionDto>.Fallo(TipoResultado.Validacion, errorPlaza);

        // 4. No se permite postularse dos veces a la misma plaza
        if (await repository.ExistePostulacionAsync(idPlaza, idCandidato))
            return ResultadoOperacion<PostulacionDto>.Fallo(TipoResultado.Conflicto, "Ya se encuentra postulado a esta plaza.");

        // 5. El candidato debe tener registrada su hoja de vida
        var hojaDeVida = await hojaDeVidaRepository.GetHojaDeVidaPorUsuarioAsync(idCandidato);
        if (hojaDeVida == null)
            return ResultadoOperacion<PostulacionDto>.Fallo(TipoResultado.Validacion, "Debe registrar su hoja de vida antes de postularse.");

        var postulacion = new Postulacion
        {
            IdPlaza = idPlaza,
            IdCandidato = idCandidato,
            FechaPostulacion = DateTime.Now,
            Estado = EstadoPostulacion.EnRevision
        };

        var id = await repository.InsertPostulacionAsync(postulacion);
        var creada = await repository.GetPostulacionByIdAsync(id);
        return ResultadoOperacion<PostulacionDto>.Correcto(mapper.Map<Postulacion, PostulacionDto>(creada ?? postulacion), "Postulación registrada correctamente.");
    }

    public async Task<ResultadoOperacion<PostulacionDto>> EvaluarAsync(int idPostulacion, EvaluarPostulacionDto dto, string? idUsuario, bool esAdministrador)
    {
        if (string.IsNullOrWhiteSpace(idUsuario))
            return ResultadoOperacion<PostulacionDto>.Fallo(TipoResultado.NoAutorizado, "Debe iniciar sesión.");

        if (!Enum.IsDefined(dto.Estado))
            return ResultadoOperacion<PostulacionDto>.Fallo(TipoResultado.Validacion, "El estado indicado no es válido.");

        var postulacion = await repository.GetPostulacionByIdAsync(idPostulacion);
        if (postulacion == null || postulacion.Plaza == null)
            return ResultadoOperacion<PostulacionDto>.Fallo(TipoResultado.NoEncontrado, "La postulación no existe.");

        if (!esAdministrador && postulacion.Plaza.IdAgente != idUsuario)
            return ResultadoOperacion<PostulacionDto>.Fallo(TipoResultado.Prohibido, "Solo el agente que publicó la plaza puede evaluar sus postulaciones.");

        if (postulacion.Plaza.Estado == EstadoPlaza.Cerrada)
            return ResultadoOperacion<PostulacionDto>.Fallo(TipoResultado.Validacion, "No se pueden evaluar postulaciones de una plaza cerrada.");

        postulacion.Estado = dto.Estado;
        postulacion.Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones) ? null : dto.Observaciones.Trim();

        await repository.UpdatePostulacionAsync(postulacion);
        var actualizada = await repository.GetPostulacionByIdAsync(idPostulacion);
        return ResultadoOperacion<PostulacionDto>.Correcto(mapper.Map<Postulacion, PostulacionDto>(actualizada!), "Postulación evaluada correctamente.");
    }

    // Regla de negocio: una plaza recibe postulaciones solo si está abierta y vigente
    public static string? ValidarPlazaDisponible(Plaza plaza, DateTime fecha)
    {
        if (plaza.Estado != EstadoPlaza.Abierta)
            return "La plaza no está abierta para recibir postulaciones.";
        if (fecha.Date < plaza.FechaPublicacion.Date)
            return "La plaza aún no ha sido publicada.";
        if (fecha.Date > plaza.FechaCierre.Date)
            return "El periodo de postulación de la plaza ya finalizó.";
        return null;
    }
}
