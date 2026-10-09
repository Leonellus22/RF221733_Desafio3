using AutoMapper;
using BolsaTrabajo.BL.Interfaces;
using BolsaTrabajo.Common;
using BolsaTrabajo.DAL.Interfaces;
using BolsaTrabajo.Entities.Dtos;
using BolsaTrabajo.Entities.Models;
using Microsoft.Extensions.Options;

namespace BolsaTrabajo.BL.Services;

// Lógica de negocio de las hojas de vida y del archivo PDF del CV
public class HojaDeVidaService(
    IHojaDeVidaRepository repository,
    IMapper mapper,
    IOptions<ArchivoSettings> archivoSettings
    ) : IHojaDeVidaService
{
    private readonly ArchivoSettings _settings = archivoSettings.Value;

    public async Task<HojaDeVidaDto?> GetHojaDeVidaPorUsuarioAsync(string idUsuario)
    {
        var result = await repository.GetHojaDeVidaPorUsuarioAsync(idUsuario);
        return result == null ? null : mapper.Map<HojaDeVida, HojaDeVidaDto>(result);
    }

    public async Task<HojaDeVidaDto?> GetHojaDeVidaByIdAsync(int id)
    {
        var result = await repository.GetHojaDeVidaByIdAsync(id);
        return result == null ? null : mapper.Map<HojaDeVida, HojaDeVidaDto>(result);
    }

    // Crea la hoja de vida si no existe o la actualiza si ya existe
    public async Task<ResultadoOperacion<HojaDeVidaDto>> GuardarHojaDeVidaAsync(string? idUsuario, HojaDeVidaDto dto)
    {
        if (string.IsNullOrWhiteSpace(idUsuario))
            return ResultadoOperacion<HojaDeVidaDto>.Fallo(TipoResultado.NoAutorizado, "Debe iniciar sesión.");

        if (string.IsNullOrWhiteSpace(dto.FormacionAcademica) || string.IsNullOrWhiteSpace(dto.ExperienciaLaboral) || string.IsNullOrWhiteSpace(dto.Competencias))
            return ResultadoOperacion<HojaDeVidaDto>.Fallo(TipoResultado.Validacion, "La formación académica, experiencia laboral y competencias son obligatorias.");

        var existente = await repository.GetHojaDeVidaPorUsuarioAsync(idUsuario);
        if (existente == null)
        {
            var nueva = new HojaDeVida
            {
                IdUsuario = idUsuario,
                FormacionAcademica = dto.FormacionAcademica.Trim(),
                ExperienciaLaboral = dto.ExperienciaLaboral.Trim(),
                Competencias = dto.Competencias.Trim()
            };
            await repository.InsertHojaDeVidaAsync(nueva);
        }
        else
        {
            existente.FormacionAcademica = dto.FormacionAcademica.Trim();
            existente.ExperienciaLaboral = dto.ExperienciaLaboral.Trim();
            existente.Competencias = dto.Competencias.Trim();
            await repository.UpdateHojaDeVidaAsync(existente);
        }

        var guardada = await repository.GetHojaDeVidaPorUsuarioAsync(idUsuario);
        return ResultadoOperacion<HojaDeVidaDto>.Correcto(mapper.Map<HojaDeVida, HojaDeVidaDto>(guardada!), "Hoja de vida guardada correctamente.");
    }

    // Valida y almacena el PDF del CV en el servidor, guardando su ruta en la base de datos
    public async Task<ResultadoOperacion<HojaDeVidaDto>> GuardarArchivoAsync(string? idUsuario, Stream contenido, string nombreArchivo, long tamano, string rutaRaiz)
    {
        if (string.IsNullOrWhiteSpace(idUsuario))
            return ResultadoOperacion<HojaDeVidaDto>.Fallo(TipoResultado.NoAutorizado, "Debe iniciar sesión.");

        var hojaDeVida = await repository.GetHojaDeVidaPorUsuarioAsync(idUsuario);
        if (hojaDeVida == null)
            return ResultadoOperacion<HojaDeVidaDto>.Fallo(TipoResultado.Validacion, "Primero debe guardar los datos de su hoja de vida.");

        if (tamano <= 0)
            return ResultadoOperacion<HojaDeVidaDto>.Fallo(TipoResultado.Validacion, "Debe seleccionar un archivo.");

        if (tamano > _settings.TamanoMaximoMB * 1024L * 1024L)
            return ResultadoOperacion<HojaDeVidaDto>.Fallo(TipoResultado.Validacion, $"El archivo no puede pesar más de {_settings.TamanoMaximoMB} MB.");

        if (!string.Equals(Path.GetExtension(nombreArchivo), ".pdf", StringComparison.OrdinalIgnoreCase))
            return ResultadoOperacion<HojaDeVidaDto>.Fallo(TipoResultado.Validacion, "Solo se permiten archivos PDF.");

        // Se verifica la firma del archivo (%PDF) para asegurar que realmente es un PDF
        using var memoria = new MemoryStream();
        await contenido.CopyToAsync(memoria);
        var bytes = memoria.ToArray();
        if (bytes.Length < 4 || bytes[0] != '%' || bytes[1] != 'P' || bytes[2] != 'D' || bytes[3] != 'F')
            return ResultadoOperacion<HojaDeVidaDto>.Fallo(TipoResultado.Validacion, "El archivo no es un PDF válido.");

        var carpeta = Path.Combine(rutaRaiz, _settings.CarpetaCV);
        Directory.CreateDirectory(carpeta);

        var nombreGuardado = $"CV_{idUsuario}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        await File.WriteAllBytesAsync(Path.Combine(carpeta, nombreGuardado), bytes);

        // Se elimina el archivo anterior para no acumular documentos
        if (!string.IsNullOrWhiteSpace(hojaDeVida.ArchivoCV))
        {
            var anterior = Path.Combine(rutaRaiz, hojaDeVida.ArchivoCV);
            if (File.Exists(anterior))
                File.Delete(anterior);
        }

        hojaDeVida.ArchivoCV = Path.Combine(_settings.CarpetaCV, nombreGuardado).Replace('\\', '/');
        await repository.UpdateHojaDeVidaAsync(hojaDeVida);

        var actualizada = await repository.GetHojaDeVidaPorUsuarioAsync(idUsuario);
        return ResultadoOperacion<HojaDeVidaDto>.Correcto(mapper.Map<HojaDeVida, HojaDeVidaDto>(actualizada!), "Archivo CV cargado correctamente.");
    }

    // Devuelve la ruta física del PDF si el usuario tiene permiso para verlo
    public async Task<ResultadoOperacion<string>> ObtenerRutaArchivoAsync(int idCV, string? idUsuario, bool puedeVerTodos, string rutaRaiz)
    {
        if (string.IsNullOrWhiteSpace(idUsuario))
            return ResultadoOperacion<string>.Fallo(TipoResultado.NoAutorizado, "Debe iniciar sesión.");

        var hojaDeVida = await repository.GetHojaDeVidaByIdAsync(idCV);
        if (hojaDeVida == null)
            return ResultadoOperacion<string>.Fallo(TipoResultado.NoEncontrado, "La hoja de vida no existe.");

        if (!puedeVerTodos && hojaDeVida.IdUsuario != idUsuario)
            return ResultadoOperacion<string>.Fallo(TipoResultado.Prohibido, "No tiene permiso para ver esta hoja de vida.");

        if (string.IsNullOrWhiteSpace(hojaDeVida.ArchivoCV))
            return ResultadoOperacion<string>.Fallo(TipoResultado.NoEncontrado, "La hoja de vida no tiene un archivo PDF cargado.");

        var ruta = Path.Combine(rutaRaiz, hojaDeVida.ArchivoCV);
        if (!File.Exists(ruta))
            return ResultadoOperacion<string>.Fallo(TipoResultado.NoEncontrado, "El archivo PDF no se encuentra en el servidor.");

        return ResultadoOperacion<string>.Correcto(ruta);
    }
}
