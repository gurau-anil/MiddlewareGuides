using System.Diagnostics;

namespace MiddlewareGuides.Middlewares
{
    public class TimingMiddleware
    {
        private readonly RequestDelegate _next;

        public TimingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            await _next(context);
            stopwatch.Stop();

            Console.WriteLine($"Request took {stopwatch.ElapsedMilliseconds} ms");
        }
    }

}
