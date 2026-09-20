namespace ITSchoolCRM.API.DTOs.Errors
{
    /// <summary>
    /// Единый формат тела ответа при ошибках.
    /// Возвращается глобальным ExceptionHandlingMiddleware
    /// и используется фронтендом для показа понятного сообщения пользователю.
    /// </summary>
    public class ErrorResponseDto
    {
        /// <summary>
        /// Машиночитаемый код ошибки, например ERR_VALIDATION или INTEGRATION_LMS_UNAVAILABLE.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Человекочитаемое сообщение об ошибке.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Идентификатор трассировки запроса (HttpContext.TraceIdentifier),
        /// по которому ошибку можно найти в логах.
        /// </summary>
        public string? TraceId { get; set; }
    }
}