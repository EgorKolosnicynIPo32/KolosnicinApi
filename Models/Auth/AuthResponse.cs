namespace Kolosnycin.Models.Auth;

public class AuthResponse
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
}