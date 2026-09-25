namespace ITSchoolCRM.API.DTOs.Auth;

/// <summary>
/// Данные для самостоятельной регистрации пользователя.
/// ФИО передаётся тремя частями (русский формат);
/// цельная строка собирается на сервере и хранится в users.full_name.
/// Пароль хранится только в Keycloak, в БД CRM не попадает.
/// </summary>
public class RegisterDto
{
    /// <summary>Фамилия.</summary>
    public string? LastName { get; set; }

    /// <summary>Имя.</summary>
    public string? FirstName { get; set; }

    /// <summary>Отчество (необязательно).</summary>
    public string? MiddleName { get; set; }

    /// <summary>Email — одновременно логин в Keycloak (users.email, unique).</summary>
    public string? Email { get; set; }

    /// <summary>Пароль. Хранится только в Keycloak.</summary>
    public string? Password { get; set; }
}