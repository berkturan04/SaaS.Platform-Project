using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SaaS.Platform.Application.Payments;
using SaaS.Platform.Domain.Entities;
using SaaS.Platform.Infrastructure.Persistance;
using SaaS.Platform.Infrastructure.Persistance.Outbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Infrastructure.BackgroundJobs
{
    public sealed class PaymentProcessorJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private const int MaxRetry = 5;

        public PaymentProcessorJob(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessAsync(stoppingToken);

                // polling interval
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task ProcessAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var paymentClient = scope.ServiceProvider.GetRequiredService<IPaymentGateway>();

            var messages = await db.Set<PaymentOutboxMessage>()
                .Where(x => !x.Processed && x.RetryCount < MaxRetry)
                .OrderBy(x => x.CreatedAt)
                .Take(20)
                .ToListAsync(ct);

            foreach (var msg in messages)
            {
                try
                {
                    await paymentClient.ChargeAsync(msg.InvoiceId, ct);

                    msg.Processed = true;
                }
                catch (Exception)
                {
                    msg.RetryCount++;
                }
            }

            await db.SaveChangesAsync(ct);
        }
    }
}
