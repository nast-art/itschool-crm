using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.AccessControl
{
    public class UserAccessItemDto
    {
        public required string KeycloakUserId { get; init; }
        public string? UserName { get; init; }
        public string? Email { get; init; }
        public string? LastName { get; init; }
        public string? FirstName { get; init; }
        public string? MiddleName { get; init; }

        /// <summary>Собранное ФИО — для отображения и отчётов.</summary>
        public required string FullName { get; init; }

        /// <summary>Роль Keycloak: user | manager | admin.</summary>
        public required string Role { get; init; }

        /// <summary>Активность учётной записи (users.is_active + Keycloak enabled).</summary>
        public required bool IsActive { get; init; }
    }
}