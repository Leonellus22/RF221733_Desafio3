using System.Net.Http.Headers;

namespace BolsaTrabajo.MVC.Services;

// Agrega automáticamente el token Bearer del usuario autenticado a cada petición hacia la API
public class TokenHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    public const string ClaimToken = "access_token";

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = httpContextAccessor.HttpContext?.User.FindFirst(ClaimToken)?.Value;
        if (request.Headers.Authorization == null && !string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return base.SendAsync(request, cancellationToken);
    }
}
