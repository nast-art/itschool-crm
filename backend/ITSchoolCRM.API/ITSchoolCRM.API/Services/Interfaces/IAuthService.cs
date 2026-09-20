using ITSchoolCRM.API.DTOs.Auth;

namespace ITSchoolCRM.API.Services.Interfaces;

public interface IAuthService
{
    Task<string> RegisterAsync(
        RegisterDto dto,
        CancellationToken cancellationToken);

    Task<AuthTokenDto?> LoginAsync(
        LoginDto dto,
        CancellationToken cancellationToken);

    /// <summary>
    /// Обмен refresh token на новую пару токенов (password grant flow).
    /// Возвращает null, если refresh token недействителен/истёк —
    /// контроллер ответит 401, фронтенд перенаправит на вход.
    /// </summary>
    Task<AuthTokenDto?> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken);
}