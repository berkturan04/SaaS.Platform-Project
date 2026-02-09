using Prometheus;
using SaaS.Platform.API.Monitoring;
using SaaS.Platform.Shared.Tenant;

namespace SaaS.Platform.API.Middlewares
{
    public sealed class TenantMetricsMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMetricsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ITenantProvider tenantProvider)
        {
            var tenant = tenantProvider.TenantId != Guid.Empty
                ? tenantProvider.TenantId.ToString()
                : "unknown";

            var endpoint = context.Request.Path.Value ?? "unknown";
            var method = context.Request.Method;

            using var timer =
                MetricsRegistry.HttpRequestDuration
                    .WithLabels(tenant, endpoint)
                    .NewTimer();

            await _next(context);

            var status = context.Response.StatusCode.ToString();

            MetricsRegistry.HttpRequestsTotal
                .WithLabels(tenant, method, endpoint, status)
                .Inc();
        }
    }
}
