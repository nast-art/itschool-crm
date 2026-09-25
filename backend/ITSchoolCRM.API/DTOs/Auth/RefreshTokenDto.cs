namespace ITSchoolCRM.API.DTOs.Auth;

/// <summary>
/// Запрос на обновление пары токенов по refresh token.
/// </summary>
public class RefreshTokenDto
{
    public string? RefreshToken { get; set; }
}