using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITSchoolCRM.API.Integrations.LMS.DTOs;

namespace ITSchoolCRM.API.Integrations.LMS.Services
{
    /// <summary>
    /// Клиент двусторонней интеграции с LMS.
    /// Реализация на заглушках: MockLmsIntegrationService.
    /// Когда заказчик предоставит реальный контракт API,
    /// достаточно заменить реализацию интерфейса, контракт сохранится.
    /// </summary>
    public interface ILmsIntegrationService
    {
        Task<List<LmsStudentDto>> GetStudentsAsync(CancellationToken cancellationToken);

        Task<List<LmsCourseDto>> GetCoursesAsync(CancellationToken cancellationToken);

        Task<List<LmsEnrollmentDto>> GetEnrollmentsAsync(CancellationToken cancellationToken);

        Task PushInteractionAsync(PushInteractionToLmsDto dto, CancellationToken cancellationToken);
    }
}