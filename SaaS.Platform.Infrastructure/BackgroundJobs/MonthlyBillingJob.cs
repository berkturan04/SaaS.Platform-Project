using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SaaS.Platform.Domain.Entities;
using SaaS.Platform.Infrastructure.Caching;
using SaaS.Platform.Infrastructure.Persistance;
using SaaS.Platform.Infrastructure.Persistance.Outbox;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Infrastructure.BackgroundJobs
{
    public class MonthlyBillingJob : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConnectionMultiplexer _redis;

        public MonthlyBillingJob(
            IServiceScopeFactory serviceScopeFactory,
            IConnectionMultiplexer redis)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _redis = redis;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);

                var periodDate = DateTime.UtcNow.AddMonths(-1);
                var year = periodDate.Year;
                var month = periodDate.Month;

                using var scope = _serviceScopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var redisDb = _redis.GetDatabase();

                // ⚠️ TEMP – Redis SCAN bir sonraki adım
                var usageEntries = new List<(Guid tenantId, int usage)>
                {
                    // mock
                };

                foreach (var (tenantId, usageCount) in usageEntries)
                {
                    var exists = await db.Set<TenantInvoice>()
                        .AnyAsync(x =>
                            x.TenantId == tenantId &&
                            x.Year == year &&
                            x.Month == month,
                            stoppingToken);

                    if (exists)
                        continue;

                    // 1️⃣ Invoice (DOMAIN)
                    var invoice = new TenantInvoice(
                        tenantId,
                        year,
                        month,
                        usageCount,
                        usageCount * 0.01m
                    );

                    db.Add(invoice);

                    // 2️⃣ Payment Outbox (INFRASTRUCTURE)
                    db.Add(new PaymentOutboxMessage
                    {
                        Id = Guid.NewGuid(),
                        InvoiceId = invoice.Id,
                        CreatedAt = DateTime.UtcNow,
                        RetryCount = 0,
                        Processed = false
                    });

                    // 3️⃣ Redis cleanup
                    var key = UsageCacheKeys.Usage(tenantId, year, month);
                    await redisDb.KeyDeleteAsync(key);
                }

                await db.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
