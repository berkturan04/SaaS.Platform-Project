using Microsoft.AspNetCore.Mvc;
using SaaS.Platform.Application.DTOs;
using SaaS.Platform.Application.Interfaces;
using SaaS.Platform.Application.Usage;
using SaaS.Platform.Domain.Entities;
using SaaS.Platform.Domain.Events;
using SaaS.Platform.Infrastructure.Caching;
using SaaS.Platform.Infrastructure.Persistance;
using SaaS.Platform.Infrastructure.Persistance.Outbox;
using SaaS.Platform.Shared.Tenant;
using System.Text.Json;

namespace SaaS.Platform.API.Controllers
{
    [ApiController]
    [Route("api/resources")]
    public class ResourcesController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly ITenantProvider _tenantProvider;
        private readonly IQuotaService _quotaService;
        private readonly IUsageTrackingService _usageTrackingService;

        public ResourcesController(
            AppDbContext dbContext,
            ITenantProvider tenantProvider,
            IQuotaService quotaService,
            IUsageTrackingService usageTrackingService)
        {
            _dbContext = dbContext;
            _tenantProvider = tenantProvider;
            _quotaService = quotaService;
            _usageTrackingService = usageTrackingService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateResourceRequest request)
        {
            // 🧪 Canary flag (Gateway → Header)
            var isCanary =
                HttpContext.Request.Headers["X-Canary"] == "true";

            // 1️⃣ Quota enforcement (hard limit)
            // Canary istersen farklı quota da verebilirsin
            if (!await _quotaService.CheckQuotaAsync())
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "API quota exceeded."
                );
            }

            // 2️⃣ Rate limit
            // Canary tenant’lara daha düşük limit uygulayabiliriz
            var rateLimit = isCanary ? 50 : 100;

            var rateLimitAllowed =
                await _usageTrackingService.CheckRateLimitAsync(
                    _tenantProvider.TenantId,
                    rateLimit,
                    60
                );

            if (!rateLimitAllowed)
            {
                return StatusCode(
                    StatusCodes.Status429TooManyRequests,
                    "Rate limit exceeded."
                );
            }

            // 3️⃣ Domain entity
            var resource = new Resource
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                TenantId = _tenantProvider.TenantId
            };

            _dbContext.Resources.Add(resource);

            // 🧪 Canary davranış farkı (örnek)
            if (isCanary)
            {
                // örn: ekstra log, yeni feature flag, farklı event vs.
                // _logger.LogInformation("Canary request processed");
            }

            // 4️⃣ Domain event → Outbox
            var @event = new ResourceCreatedEvent(
                resource.Id,
                resource.TenantId,
                resource.Name
            );

            _dbContext.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredOnUtc = DateTime.UtcNow,
                Type = @event.GetType().FullName!,
                Payload = JsonSerializer.Serialize(@event)
            });

            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                resource.Id,
                Canary = isCanary
            });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var resource = await _dbContext.Resources.FindAsync(id);

            if (resource == null)
                return NotFound();

            resource.Delete();
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
