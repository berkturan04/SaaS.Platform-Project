using Prometheus;
using SaaS.Platform.API.Monitoring;

namespace SaaS.Platform.API.Middlewares
{
    public sealed class CanaryMetricsMiddleware
    {
        private readonly RequestDelegate _next;

        public CanaryMetricsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var isCanary =
                context.Request.Headers.TryGetValue("X-Canary", out var value)
                && value == "true";

            if (isCanary)
            {
                CanaryMetrics.Requests
                    .WithLabels("canary")
                    .Inc();
            }

            try
            {
                if (isCanary)
                {
                    using (CanaryMetrics.Latency
                        .WithLabels("canary")
                        .NewTimer())
                    {
                        await _next(context);
                    }
                }
                else
                {
                    await _next(context);
                }
            }
            catch
            {
                if (isCanary)
                {
                    CanaryMetrics.Errors
                        .WithLabels("canary")
                        .Inc();
                }

                throw;
            }
        }
    }
}
