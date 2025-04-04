using System.Net;
using System.Security.Authentication;
using System.Text.Json;

namespace MiddlewareGuides.Middlewares
{
    public class ExceptionHandlingMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, _logger);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger<ExceptionHandlingMiddleware> logger)
        {
            HttpStatusCode statusCode;
            string message;

            switch (exception)
            {
                case AuthenticationException ex:
                    statusCode = HttpStatusCode.Unauthorized;
                    message = "Authentication failed.";
                    logger.LogWarning(exception, "Authentication failure.");
                    break;
                case UnauthorizedAccessException ex:
                    statusCode = HttpStatusCode.Unauthorized;
                    message = "You are not authorized to access this resource.";
                    logger.LogWarning(exception, "Unauthorized access attempt.");
                    break;
                case KeyNotFoundException _:
                    statusCode = HttpStatusCode.NotFound;
                    message = "The requested resource was not found.";
                    logger.LogWarning(exception, "Resource not found.");
                    break;
                case ArgumentException _:
                    statusCode = HttpStatusCode.BadRequest;
                    message = "Invalid request parameters.";
                    logger.LogWarning(exception, "Invalid request.");
                    break;
                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    message = "An unexpected error occurred. Please try again later.";
                    logger.LogError(exception, "Unhandled exception occurred.");
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var errorResponse = new { StatusCode = (int)statusCode, Message = message };
            var jsonResponse = JsonSerializer.Serialize(errorResponse);

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
