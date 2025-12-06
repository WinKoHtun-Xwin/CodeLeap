namespace CodeLeap.Application.DTOs.Auth;

public class GetAuthDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public required string UserId { get; set; }
    public required string Username { get; set; }
    public required DateTime ExpiresAt { get; set; }
}
