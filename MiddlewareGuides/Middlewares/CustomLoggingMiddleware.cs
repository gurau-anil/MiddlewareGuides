namespace MiddlewareGuides.Middlewares
{
    public class CustomLoggingMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly ILogger<CustomLoggingMiddleware> _logger;

        public CustomLoggingMiddleware(RequestDelegate next, ILogger<CustomLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation($"Request started: {context.Request.Path}");
            await _next(context); // Call the next middleware in the pipeline
            _logger.LogInformation($"Request finished: {context.Request.Path} - Status: {context.Response.StatusCode}");
        }
    }
}
