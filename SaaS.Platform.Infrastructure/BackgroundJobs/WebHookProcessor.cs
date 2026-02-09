using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SaaS.Platform.Domain.Entities;
using SaaS.Platform.Infrastructure.Payments;
using SaaS.Platform.Infrastructure.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SaaS.Platform.Infrastructure.BackgroundJobs
{
    public sealed class WebhookProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public WebhookProcessor(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task ProcessAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var events = await db.Set<WebhookEvent>()
                .Where(x => !x.Processed)
                .OrderBy(x => x.ReceivedAt)
                .Take(20)
                .ToListAsync(ct);

            foreach (var evt in events)
            {
                if (evt.EventType == "payment.succeeded")
                {
                    var invoiceId =
                        JsonDocument.Parse(evt.Payload)
                            .RootElement
                            .GetProperty("invoiceId")
                            .GetGuid();

                    var invoice = await db.Set<TenantInvoice>()
                        .FirstOrDefaultAsync(x => x.Id == invoiceId, ct);

                    if (invoice != null)
                    {
                        invoice.MarkPaid(); // domain method
                    }
                }

                evt.Processed = true;
            }

            await db.SaveChangesAsync(ct);
        }
    }
}
