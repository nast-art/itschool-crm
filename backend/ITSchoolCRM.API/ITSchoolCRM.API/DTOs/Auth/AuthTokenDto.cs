namespace ITSchoolCRM.API.DTOs.Auth;

public class AuthTokenDto
{
    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public int ExpiresIn { get; set; }
}