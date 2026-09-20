using ITSchoolCRM.API.Auth;
using ITSchoolCRM.API.Integrations.Common;
using ITSchoolCRM.API.Integrations.LMS.DTOs;
using ITSchoolCRM.API.Integrations.LMS.Services;
using ITSchoolCRM.API.Integrations.Website.DTOs;
using ITSchoolCRM.API.Integrations.Website.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSchoolCRM.API.Integrations.Controllers
{
    /// <summary>
    /// Фасад интеграций для фронтенда CRM.
    /// Оборачивает вызовы к внешним системам (LMS и сайт),
    /// единообразно превращая IntegrationException в HTTP 502 с кодом ошибки.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class IntegrationsController : ControllerBase
    {
        private readonly ILmsIntegrationService _lmsIntegrationService;

        private readonly IWebsiteIntegrationService _websiteIntegrationService;

        public IntegrationsController(
            ILmsIntegrationService lmsIntegrationService,
            IWebsiteIntegrationService websiteIntegrationService)
        {
            _lmsIntegrationService = lmsIntegrationService;
            _websiteIntegrationService = websiteIntegrationService;
        }

        // ---------------------------------------------------------
        // LMS
        // ---------------------------------------------------------

        [HttpGet("lms/students")]
        [Authorize(Policy = Policies.UserAccess)]
        public async Task<ActionResult<List<LmsStudentDto>>> GetLmsStudents(
            CancellationToken cancellationToken)
        {
            try
            {
                var students =
                    await _lmsIntegrationService.GetStudentsAsync(
                        cancellationToken);

                return Ok(students);
            }
            catch (IntegrationException ex)
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    new
                    {
                        code = ex.ErrorCode,
                        message = ex.Message
                    });
            }
        }

        [HttpGet("lms/courses")]
        [Authorize(Policy = Policies.UserAccess)]
        public async Task<ActionResult<List<LmsCourseDto>>> GetLmsCourses(
            CancellationToken cancellationToken)
        {
            try
            {
                var courses =
                    await _lmsIntegrationService.GetCoursesAsync(
                        cancellationToken);

                return Ok(courses);
            }
            catch (IntegrationException ex)
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    new
                    {
                        code = ex.ErrorCode,
                        message = ex.Message
                    });
            }
        }

        [HttpGet("lms/enrollments")]
        [Authorize(Policy = Policies.UserAccess)]
        public async Task<ActionResult<List<LmsEnrollmentDto>>> GetLmsEnrollments(
            CancellationToken cancellationToken)
        {
            try
            {
                var enrollments =
                    await _lmsIntegrationService.GetEnrollmentsAsync(
                        cancellationToken);

                return Ok(enrollments);
            }
            catch (IntegrationException ex)
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    new
                    {
                        code = ex.ErrorCode,
                        message = ex.Message
                    });
            }
        }

        [HttpPost("lms/interactions")]
        [Authorize(Policy = Policies.UserAccess)]
        public async Task<IActionResult> PushInteractionToLms(
            [FromBody] PushInteractionToLmsDto dto,
            CancellationToken cancellationToken)
        {
            try
            {
                await _lmsIntegrationService.PushInteractionAsync(
                    dto,
                    cancellationToken);

                return NoContent();
            }
            catch (IntegrationException ex)
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    new
                    {
                        code = ex.ErrorCode,
                        message = ex.Message
                    });
            }
        }

        // ---------------------------------------------------------
        // Website
        // ---------------------------------------------------------

        [HttpGet("website/applications")]
        [Authorize(Policy = Policies.UserAccess)]
        public async Task<ActionResult<List<WebsiteApplicationDto>>> GetWebsiteApplications(
            CancellationToken cancellationToken)
        {
            try
            {
                var applications =
                    await _websiteIntegrationService.GetApplicationsAsync(
                        cancellationToken);

                return Ok(applications);
            }
            catch (IntegrationException ex)
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    new
                    {
                        code = ex.ErrorCode,
                        message = ex.Message
                    });
            }
        }

        [HttpPost("website/interactions")]
        [Authorize(Policy = Policies.UserAccess)]
        public async Task<IActionResult> PushInteractionToWebsite(
            [FromBody] PushInteractionToWebsiteDto dto,
            CancellationToken cancellationToken)
        {
            try
            {
                await _websiteIntegrationService.PushInteractionAsync(
                    dto,
                    cancellationToken);

                return NoContent();
            }
            catch (IntegrationException ex)
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    new
                    {
                        code = ex.ErrorCode,
                        message = ex.Message
                    });
            }
        }
    }
}