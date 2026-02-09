using Microsoft.Extensions.Caching.Distributed;
using SaaS.Platform.Application.Usage;
using SaaS.Platform.Shared.Tenant;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Infrastructure.Caching
{
    public sealed class UsageTrackingService : IUsageTrackingService
    {
        private readonly IDatabase _redis;
        private readonly ITenantProvider _tenantProvider;

        public UsageTrackingService(
            IConnectionMultiplexer redis,
            ITenantProvider tenantProvider)
        {
            _redis = redis.GetDatabase();
            _tenantProvider = tenantProvider;
        }

        // Controller sadece bunu çağıracak
        public async Task TrackAsync()
        {
            var tenantId = _tenantProvider.TenantId;
            var now = DateTime.UtcNow;

            var key = UsageCacheKeys.Usage(
                tenantId,
                now.Year,
                now.Month);

            var count = await _redis.StringIncrementAsync(key);

            if (count == 1)
            {
                await _redis.KeyExpireAsync(
                    key,
                    TimeSpan.FromDays(40));
            }
        }

        // Job'lar ve future use için
        public async Task IncrementAsync(Guid tenantId, int year, int month)
        {
            var key = UsageCacheKeys.Usage(tenantId, year, month);

            // Atomic increment
            var value = await _redis.StringIncrementAsync(key);

            // Expire sadece ilk create anında set edilir
            if (value == 1)
            {
                // Ay bazlı usage → 40 gün güvenli
                await _redis.KeyExpireAsync(
                    key,
                    TimeSpan.FromDays(40)
                );
            }
        }
        // Rate limiting kontrolü
        public async Task<bool> CheckRateLimitAsync(Guid tenantId, int limit, int periodInSeconds)
        {
            var key = $"rate_limit:{tenantId}:{DateTime.UtcNow.Ticks / periodInSeconds}";

            var usageCount = await _redis.StringGetAsync(key);
            if (!usageCount.HasValue)
            {
                await _redis.StringSetAsync(key, 1, TimeSpan.FromSeconds(periodInSeconds));
                return true; // first request
            }

            if ((int)usageCount < limit)
            {
                await _redis.StringIncrementAsync(key);
                return true; // within limit
            }

            return false; // rate limit exceeded
        }
        // Quota kontrolü
        public async Task<bool> CheckQuotaAsync(Guid tenantId, int limit)
        {
            var key = $"quota:{tenantId}";

            var currentUsage = await _redis.StringGetAsync(key);
            if (!currentUsage.HasValue)
            {
                await _redis.StringSetAsync(key, 1, TimeSpan.FromDays(30));  // first request in a new period
                return true; // quota not exceeded
            }

            if ((int)currentUsage < limit)
            {
                await _redis.StringIncrementAsync(key);
                return true; // within quota
            }

            return false; // quota exceeded
        }
    }
}
