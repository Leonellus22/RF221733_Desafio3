using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BolsaTrabajo.Entities.Dtos;

namespace BolsaTrabajo.MVC.Services;

// Cliente HTTP que consume los endpoints de BolsaTrabajo.API
public class ApiClient(HttpClient http)
{
    // La API envía los enums como texto, por eso se agrega el convertidor
    private static readonly JsonSerializerOptions Opciones = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public Task<RespuestaApi<T>> GetAsync<T>(string url, string? token = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (token != null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return EnviarAsync<T>(request);
    }

    public Task<RespuestaApi<T>> PostAsync<T>(string url, object? cuerpo)
    {
        return EnviarAsync<T>(new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(cuerpo, options: Opciones) });
    }

    public Task<RespuestaApi<T>> PutAsync<T>(string url, object? cuerpo)
    {
        return EnviarAsync<T>(new HttpRequestMessage(HttpMethod.Put, url) { Content = JsonContent.Create(cuerpo, options: Opciones) });
    }

    public Task<RespuestaApi<T>> DeleteAsync<T>(string url)
    {
        return EnviarAsync<T>(new HttpRequestMessage(HttpMethod.Delete, url));
    }

    // Envía un archivo como multipart/form-data
    public async Task<RespuestaApi<T>> PostArchivoAsync<T>(string url, IFormFile archivo, string nombreCampo)
    {
        var contenido = new MultipartFormDataContent();
        var stream = new StreamContent(archivo.OpenReadStream());
        stream.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrEmpty(archivo.ContentType) ? "application/pdf" : archivo.ContentType);
        contenido.Add(stream, nombreCampo, archivo.FileName);
        return await EnviarAsync<T>(new HttpRequestMessage(HttpMethod.Post, url) { Content = contenido });
    }

    // Inicia sesión en el endpoint de Identity y devuelve el token
    public async Task<RespuestaApi<TokenDto>> LoginAsync(LoginDto dto)
    {
        var respuesta = await PostAsync<TokenDto>("api/auth/login", new { email = dto.Email, password = dto.Password });
        if (respuesta.Codigo == HttpStatusCode.Unauthorized)
            respuesta.Mensaje = "Correo o contraseña incorrectos.";
        return respuesta;
    }

    // Descarga un archivo (CV en PDF)
    public async Task<ArchivoApi> GetArchivoAsync(string url)
    {
        try
        {
            using var response = await http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var texto = await response.Content.ReadAsStringAsync();
                return new ArchivoApi { Mensaje = ObtenerMensajeError(response.StatusCode, texto) };
            }

            return new ArchivoApi
            {
                Exito = true,
                Contenido = await response.Content.ReadAsByteArrayAsync(),
                TipoContenido = response.Content.Headers.ContentType?.MediaType ?? "application/pdf",
                NombreArchivo = response.Content.Headers.ContentDisposition?.FileNameStar
                                ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                                ?? "CV.pdf"
            };
        }
        catch (HttpRequestException)
        {
            return new ArchivoApi { Mensaje = MensajeSinConexion };
        }
    }

    private const string MensajeSinConexion = "No se pudo conectar con la API. Verifique que el proyecto BolsaTrabajo.API esté en ejecución.";

    private async Task<RespuestaApi<T>> EnviarAsync<T>(HttpRequestMessage request)
    {
        try
        {
            using var response = await http.SendAsync(request);
            var texto = await response.Content.ReadAsStringAsync();
            var resultado = new RespuestaApi<T> { Codigo = response.StatusCode, Exito = response.IsSuccessStatusCode };

            if (response.IsSuccessStatusCode)
            {
                if (!string.IsNullOrWhiteSpace(texto) && response.StatusCode != HttpStatusCode.NoContent)
                    resultado.Datos = JsonSerializer.Deserialize<T>(texto, Opciones);
            }
            else
            {
                resultado.Mensaje = ObtenerMensajeError(response.StatusCode, texto);
            }
            return resultado;
        }
        catch (HttpRequestException)
        {
            return new RespuestaApi<T> { Codigo = HttpStatusCode.ServiceUnavailable, Mensaje = MensajeSinConexion };
        }
    }

    // Obtiene el mensaje de error que envía la API ({ mensaje } o ValidationProblemDetails)
    private static string ObtenerMensajeError(HttpStatusCode codigo, string texto)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(texto))
            {
                using var json = JsonDocument.Parse(texto);
                var raiz = json.RootElement;
                if (raiz.ValueKind == JsonValueKind.Object)
                {
                    if (raiz.TryGetProperty("mensaje", out var mensaje))
                        return mensaje.GetString() ?? string.Empty;

                    if (raiz.TryGetProperty("errors", out var errores) && errores.ValueKind == JsonValueKind.Object)
                    {
                        var lista = errores.EnumerateObject()
                            .SelectMany(e => e.Value.EnumerateArray().Select(v => v.GetString()))
                            .Where(m => !string.IsNullOrWhiteSpace(m));
                        return string.Join(" ", lista);
                    }
                }
            }
        }
        catch (JsonException)
        {
            // La respuesta no es JSON; se usa el mensaje genérico
        }

        return codigo switch
        {
            HttpStatusCode.Unauthorized => "Su sesión expiró. Inicie sesión nuevamente.",
            HttpStatusCode.Forbidden => "No tiene permiso para realizar esta acción.",
            HttpStatusCode.NotFound => "El recurso solicitado no existe.",
            _ => "Ocurrió un error al procesar la solicitud."
        };
    }
}
