using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.Services.Interfaces
{
    public interface IUserAccessService
    {
        Task<int?> GetCurrentDatabaseUserIdAsync(CancellationToken cancellationToken);

        Task<bool> HasAccessToUniversityAsync(int universityId, CancellationToken cancellationToken);

        IQueryable<int> GetAccessibleUniversityIds();

        bool HasFullAccess();
    }
}