namespace BolsaTrabajo.Entities.Dtos;

// Respuesta del endpoint /api/auth/login de ASP.NET Core Identity
public class TokenDto
{
    public string TokenType { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public long ExpiresIn { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
}
