using Microsoft.Extensions.Caching.Memory;

namespace MiddlewareGuides.Middlewares
{
    public class ApiKeyRateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;

        public ApiKeyRateLimitingMiddleware(RequestDelegate next, IMemoryCache cache, IConfiguration configuration)
        {
            _next = next;
            _cache = cache;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string apiKey = context.Request.Headers["X-API-Key"];

            if (string.IsNullOrEmpty(apiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key is missing.");
                return;
            }

            int rateLimit = _configuration.GetValue<int>("ApiKeyDailyRateLimit:Limit");
            DateTime today = DateTime.UtcNow.Date;
            string cacheKey = $"api-key-daily-rate-limit-{apiKey}-{today:yyyyMMdd}";

            int accessCount = _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = today.AddDays(1).Subtract(DateTime.UtcNow);
                return 0;
            });

            if (accessCount >= rateLimit)
            {
                context.Response.StatusCode = 429;
                await context.Response.WriteAsync("Daily API rate limit exceeded.");
                return;
            }

            // Call the next middleware
            await _next(context);

            // Check for successful response status codes (2xx)
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                // Increment count only for successful responses
                await UpdateAccessCountAsync(cacheKey, accessCount + 1, today);
                context.Response.Headers["X-RateLimit-Remaining"] = (rateLimit - (accessCount + 1)).ToString();

            }

        }

        private async Task UpdateAccessCountAsync(string cacheKey, int newCount, DateTime today)
        {
            await Task.Delay(50);
            _cache.Set(cacheKey, newCount, today.AddDays(1).Subtract(DateTime.UtcNow));
        }
    }
}
