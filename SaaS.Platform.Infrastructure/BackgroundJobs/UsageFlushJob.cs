using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SaaS.Platform.Application.Usage;
using SaaS.Platform.Domain.Entities;
using SaaS.Platform.Infrastructure.Caching;
using SaaS.Platform.Infrastructure.Persistance;
using SaaS.Platform.Infrastructure.Redis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Infrastructure.BackgroundJobs
{
    public sealed class UsageFlushJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConnectionMultiplexer _redis;

        public UsageFlushJob(
            IServiceScopeFactory scopeFactory,
            IConnectionMultiplexer redis)
        {
            _scopeFactory = scopeFactory;
            _redis = redis;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(
                    TimeSpan.FromMinutes(1),
                    stoppingToken);

                await FlushAsync(stoppingToken);
            }
        }

        private async Task FlushAsync(
            CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

            var server = _redis
                .GetServer(_redis.GetEndPoints().First());

            var redisDb = _redis.GetDatabase();

            var now = DateTime.UtcNow;
            var period =
                new DateTime(now.Year, now.Month, 1);

            await foreach (var key in server.KeysAsync(
                pattern: "usage:*"))
            {
                var value =
                    await redisDb.StringGetAsync(key);

                if (!value.HasValue)
                    continue;

                var parts = key.ToString().Split(':');
                var tenantId = Guid.Parse(parts[1]);

                var count = (int)value;

                var usage =
                    await db.Set<TenantUsage>()
                        .FirstOrDefaultAsync(
                            x => x.TenantId == tenantId
                              && x.Period == period,
                            stoppingToken);

                if (usage == null)
                {
                    db.Add(new TenantUsage
                    {
                        TenantId = tenantId,
                        Period = period,
                        RequestCount = count
                    });
                }
                else
                {
                    usage.RequestCount += count;
                }

                await redisDb.KeyDeleteAsync(key);
            }

            await db.SaveChangesAsync(stoppingToken);
        }
    }

}
