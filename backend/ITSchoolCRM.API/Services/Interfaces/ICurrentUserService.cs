using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.Services.Interfaces
{
    public interface ICurrentUserService
    {
        bool IsAuthenticated { get; }

        string? KeycloakUserId { get; }

        string? UserName { get; }

        string? Email { get; }

        string? FullName { get; }

        IReadOnlyList<string> Roles { get; }

        bool IsUser { get; }

        bool IsManager { get; }

        bool IsAdmin { get; }
    }
}