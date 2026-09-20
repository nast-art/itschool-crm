using System.Collections.Concurrent;
using ITSchoolCRM.API.Integrations.Common;
using ITSchoolCRM.API.Integrations.LMS.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ITSchoolCRM.API.Integrations.LMS.Controllers
{
    /// <summary>
    /// Заглушка внешней LMS.
    /// Имитирует эндпоинты реальной LMS:
    ///  — GET  выдают справочные данные (обучающиеся, курсы, записи на курсы);
    ///  — POST принимает данные взаимодействия из CRM (направление CRM → LMS).
    /// Доступ защищён заголовком X-Api-Key, проверяемым по конфигурации.
    /// Когда заказчик предоставит реальную LMS, этот контроллер удаляется,
    /// а MockLmsIntegrationService заменяется на реальный клиент.
    /// </summary>
    [ApiController]
    [Route("api/mock-lms")]
    public class LmsMockController : ControllerBase
    {
        private static readonly ConcurrentDictionary<int, LmsStudentDto> Students =
            new()
            {
                [1] = new LmsStudentDto
                {
                    Id = 1,
                    FullName = "Петров Василий Сергеевич",
                    Email = "v.petrov@example.ru",
                    UniversityName = "Московский государственный университет",
                    ProgramName = "DevOps-инженер",
                    IsActive = true
                },
                [2] = new LmsStudentDto
                {
                    Id = 2,
                    FullName = "Сидорова Анна Викторовна",
                    Email = "a.sidorova@example.ru",
                    UniversityName = "Санкт-Петербургский государственный университет",
                    ProgramName = "Тестировщик ПО (QA)",
                    IsActive = true
                },
                [3] = new LmsStudentDto
                {
                    Id = 3,
                    FullName = "Козлов Дмитрий Андреевич",
                    Email = "d.kozlov@example.ru",
                    UniversityName = "Национальный исследовательский университет МИЭТ",
                    ProgramName = "Инженер данных",
                    IsActive = true
                }
            };

        private static readonly ConcurrentDictionary<int, LmsCourseDto> Courses =
            new()
            {
                [1] = new LmsCourseDto
                {
                    Id = 1,
                    Name = "DevOps-инженер",
                    DirectionName = "DevOps",
                    IsActive = true
                },
                [2] = new LmsCourseDto
                {
                    Id = 2,
                    Name = "Тестировщик ПО (QA)",
                    DirectionName = "QA",
                    IsActive = true
                },
                [3] = new LmsCourseDto
                {
                    Id = 3,
                    Name = "Инженер данных",
                    DirectionName = "Data Engineering",
                    IsActive = true
                }
            };

        private static readonly ConcurrentDictionary<int, LmsEnrollmentDto> Enrollments =
            new()
            {
                [1] = new LmsEnrollmentDto
                {
                    Id = 1,
                    StudentId = 1,
                    StudentFullName = "Петров Василий Сергеевич",
                    CourseId = 1,
                    CourseName = "DevOps-инженер",
                    Status = "InProgress",
                    ProgressPercent = 45,
                    EnrolledAt = new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc)
                },
                [2] = new LmsEnrollmentDto
                {
                    Id = 2,
                    StudentId = 2,
                    StudentFullName = "Сидорова Анна Викторовна",
                    CourseId = 2,
                    CourseName = "Тестировщик ПО (QA)",
                    Status = "Completed",
                    ProgressPercent = 100,
                    EnrolledAt = new DateTime(2026, 8, 15, 9, 0, 0, DateTimeKind.Utc)
                }
            };

        private static readonly List<PushInteractionToLmsDto> ReceivedInteractions =
            new();

        private static readonly object ReceivedInteractionsLock =
            new();

        private readonly IConfiguration _configuration;

        private readonly ILogger<LmsMockController> _logger;

        public LmsMockController(
            IConfiguration configuration,
            ILogger<LmsMockController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("students")]
        public ActionResult<List<LmsStudentDto>> GetStudents()
        {
            if (!IsAuthorized())
            {
                return Unauthorized(new
                {
                    code = IntegrationErrorCodes.ConfigurationInvalid,
                    message = "Некорректный или отсутствующий API-ключ."
                });
            }

            return Ok(Students.Values.ToList());
        }

        [HttpGet("courses")]
        public ActionResult<List<LmsCourseDto>> GetCourses()
        {
            if (!IsAuthorized())
            {
                return Unauthorized(new
                {
                    code = IntegrationErrorCodes.ConfigurationInvalid,
                    message = "Некорректный или отсутствующий API-ключ."
                });
            }

            return Ok(Courses.Values.ToList());
        }

        [HttpGet("enrollments")]
        public ActionResult<List<LmsEnrollmentDto>> GetEnrollments()
        {
            if (!IsAuthorized())
            {
                return Unauthorized(new
                {
                    code = IntegrationErrorCodes.ConfigurationInvalid,
                    message = "Некорректный или отсутствующий API-ключ."
                });
            }

            return Ok(Enrollments.Values.ToList());
        }

        /// <summary>
        /// Приём данных взаимодействия из CRM (направление CRM → LMS).
        /// </summary>
        [HttpPost("interactions")]
        public ActionResult ReceiveInteraction(
            [FromBody] PushInteractionToLmsDto dto)
        {
            if (!IsAuthorized())
            {
                return Unauthorized(new
                {
                    code = IntegrationErrorCodes.ConfigurationInvalid,
                    message = "Некорректный или отсутствующий API-ключ."
                });
            }

            if (dto is null || dto.InteractionId <= 0)
            {
                return BadRequest(new
                {
                    code = IntegrationErrorCodes.LmsBadResponse,
                    message = "Тело запроса не содержит корректного идентификатора взаимодействия."
                });
            }

            lock (ReceivedInteractionsLock)
            {
                ReceivedInteractions.Add(dto);
            }

            _logger.LogInformation(
                "Mock LMS принял взаимодействие {InteractionId} (ВУЗ: {UniversityName}, статус: {StatusName}).",
                dto.InteractionId,
                dto.UniversityName,
                dto.StatusName);

            return Ok(new
            {
                received = true,
                interactionId = dto.InteractionId,
                receivedAt = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Служебный эндпоинт для проверки двустороннего обмена:
        /// показывает, какие взаимодействия LMS получила из CRM.
        /// </summary>
        [HttpGet("received-interactions")]
        public ActionResult<List<PushInteractionToLmsDto>> GetReceivedInteractions()
        {
            if (!IsAuthorized())
            {
                return Unauthorized(new
                {
                    code = IntegrationErrorCodes.ConfigurationInvalid,
                    message = "Некорректный или отсутствующий API-ключ."
                });
            }

            lock (ReceivedInteractionsLock)
            {
                return Ok(ReceivedInteractions.ToList());
            }
        }

        private bool IsAuthorized()
        {
            var expectedKey =
                _configuration["Integrations:MockApiKey"];

            if (string.IsNullOrWhiteSpace(expectedKey))
            {
                return true;
            }

            var providedKey =
                Request.Headers["X-Api-Key"].ToString();

            return string.Equals(
                providedKey,
                expectedKey,
                StringComparison.Ordinal);
        }
    }
}