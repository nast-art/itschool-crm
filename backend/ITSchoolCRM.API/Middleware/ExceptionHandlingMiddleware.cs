using System.Text.Json;
using ITSchoolCRM.API.DTOs.Errors;
using ITSchoolCRM.API.Integrations.Common;

namespace ITSchoolCRM.API.Middleware
{
    /// <summary>
    /// Глобальный middleware обработки исключений.
    /// Перехватывает все необработанные исключения, возникающие ниже по конвейеру
    /// (в контроллерах и сервисах), превращает их в единый JSON-ответ
    /// с машиночитаемым кодом ошибки (требование ТЗ: "Должны быть предусмотрены коды ошибок").
    ///
    /// Соглашение по кодам:
    ///   ERR_VALIDATION       — некорректные входные данные (ArgumentException, ArgumentNullException);
    ///   ERR_BAD_REQUEST      — некорректный HTTP-запрос (BadHttpRequestException);
    ///   ERR_NOT_FOUND        — сущность не найдена (KeyNotFoundException);
    ///   ERR_CONFLICT         — нарушение бизнес-правила (InvalidOperationException);
    ///   ERR_FORBIDDEN        — нет доступа к данным (UnauthorizedAccessException);
    ///   INTEGRATION_*        — сбои обращений к внешним системам (IntegrationException, HTTP 502);
    ///   ERR_INTERNAL         — непредвиденная ошибка, HTTP 500.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(
                    context,
                    ex);
            }
        }

        private async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception)
        {
            var (statusCode, errorCode, message) =
                MapException(exception);

            // Внутренние детали непредвиденных ошибок пишем только в лог,
            // наружу — обезличенное сообщение.
            if (statusCode >= 500)
            {
                _logger.LogError(
                    exception,
                    "Необработанное исключение при обработке запроса {Method} {Path}. Код: {ErrorCode}",
                    context.Request.Method,
                    context.Request.Path,
                    errorCode);
            }
            else
            {
                _logger.LogWarning(
                    "Запрос {Method} {Path} завершился ошибкой {StatusCode} ({ErrorCode}): {Message}",
                    context.Request.Method,
                    context.Request.Path,
                    statusCode,
                    errorCode,
                    exception.Message);
            }

            // Если клиент уже начал получать ответ (например, при стриминге
            // или выгрузке файла), статус-код и тело менять нельзя —
            // только фиксируем ошибку в логе.
            if (context.Response.HasStarted)
            {
                _logger.LogWarning(
                    "Ответ уже начат, код ошибки {ErrorCode} не может быть отправлен клиенту.",
                    errorCode);

                return;
            }

            var response = new ErrorResponseDto
            {
                Code = errorCode,
                Message = message,
                TraceId = context.TraceIdentifier
            };

            context.Response.StatusCode = statusCode;

            context.Response.ContentType =
                "application/json; charset=utf-8";

            var json =
                JsonSerializer.Serialize(
                    response,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy =
                            JsonNamingPolicy.CamelCase
                    });

            try
            {
                await context.Response.WriteAsync(json);
            }
            catch (Exception writeEx)
            {
                _logger.LogError(
                    writeEx,
                    "Не удалось записать тело ошибки в ответ.");
            }
        }

        /// <summary>
        /// Сопоставляет исключение со статус-кодом HTTP, кодом ошибки и сообщением.
        /// Порядок проверок важен: сначала самые специфичные типы.
        /// </summary>
        private static (
            int StatusCode,
            string ErrorCode,
            string Message) MapException(
            Exception exception)
        {
            switch (exception)
            {
                case IntegrationException integrationException:

                    return (
                        StatusCodes.Status502BadGateway,
                        integrationException.ErrorCode,
                        integrationException.Message);

                case UnauthorizedAccessException:

                    return (
                        StatusCodes.Status403Forbidden,
                        "ERR_FORBIDDEN",
                        exception.Message);

                case KeyNotFoundException:

                    return (
                        StatusCodes.Status404NotFound,
                        "ERR_NOT_FOUND",
                        exception.Message);

                case InvalidOperationException:

                    return (
                        StatusCodes.Status409Conflict,
                        "ERR_CONFLICT",
                        exception.Message);

                case ArgumentNullException:
                case ArgumentException:

                    return (
                        StatusCodes.Status400BadRequest,
                        "ERR_VALIDATION",
                        exception.Message);

                case BadHttpRequestException:

                    return (
                        StatusCodes.Status400BadRequest,
                        "ERR_BAD_REQUEST",
                        "Некорректный формат HTTP-запроса.");

                default:

                    return (
                        StatusCodes.Status500InternalServerError,
                        "ERR_INTERNAL",
                        "Внутренняя ошибка сервера. Обратитесь к администратору платформы.");
            }
        }
    }
}