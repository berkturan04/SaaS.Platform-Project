using SaaS.Platform.Application.Usage;
using SaaS.Platform.Shared.Tenant;

namespace SaaS.Platform.API.Middlewares
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider, IUsageCollector usageCollector)
        {
            if (!context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader))
                throw new UnauthorizedAccessException("Tenant Header Missing!");

            if (!Guid.TryParse(tenantIdHeader.ToString(), out var tenantId))
                throw new UnauthorizedAccessException("Invalid Tenant Id!");

            tenantProvider.SetTenant(tenantId);

            await _next(context);
            usageCollector.Increment(tenantId);
        }
    }
}
