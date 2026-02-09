using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaaS.Platform.Infrastructure.Payments;
using SaaS.Platform.Infrastructure.Persistance;
using System.Text.Json;

namespace SaaS.Platform.API.Controllers
{
    [ApiController]
    [Route("api/webhooks/payment")]
    public sealed class PaymentWebhookController : ControllerBase
    {
        private readonly AppDbContext _db;

        public PaymentWebhookController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Receive(
            [FromHeader(Name = "X-Event-Id")] string eventId,
            [FromHeader(Name = "X-Event-Type")] string eventType,
            [FromBody] JsonElement payload)
        {
            // 1️⃣ Idempotency guard
            var exists = await _db.Set<WebhookEvent>()
                .AnyAsync(x => x.ProviderEventId == eventId);

            if (exists)
                return Ok(); // duplicate → ACK

            // 2️⃣ Persist FIRST
            var webhookEvent = new WebhookEvent
            {
                Id = Guid.NewGuid(),
                ProviderEventId = eventId,
                EventType = eventType,
                Payload = payload.GetRawText(),
                ReceivedAt = DateTime.UtcNow,
                Processed = false
            };

            _db.Add(webhookEvent);
            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}
