using Serilog.Context;

namespace UserApp.Infrastructure.Features
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var requestId = Guid.NewGuid().ToString();
            using (LogContext.PushProperty("RequestId", requestId))
            {
                await _next(context);
            }
        }
    }

}
