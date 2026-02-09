using SaaS.Platform.Application.Usage;
using SaaS.Platform.Infrastructure.Caching;
using SaaS.Platform.Infrastructure.Redis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Infrastructure.Usage
{
    public sealed class RedisUsageTrackingService : IUsageTrackingService
    {
        private readonly IDatabase _db;

        public RedisUsageTrackingService()
        {
            _db = RedisConnection.Instance.GetDatabase();
        }

        // --------------------------------------------------
        // 1️ Atomic increment (usage counter)
        // --------------------------------------------------
        public Task IncrementAsync(Guid tenantId, int year, int month)
        {
            var key = UsageCacheKeys.Usage(tenantId, year, month);
            return _db.StringIncrementAsync(key, 1);
        }

        // --------------------------------------------------
        // 2️⃣Track (request bazlı – controller içinden çağrılır)
        // --------------------------------------------------
        public async Task TrackAsync()
        {
            // Bu metod controller seviyesinde çağrılıyor
            // Asıl iş IncrementAsync / CheckRateLimitAsync içinde
            await Task.CompletedTask;
        }

        // --------------------------------------------------
        // 3️ Rate limit check (Redis INCR + EXPIRE)
        // --------------------------------------------------
        public async Task<bool> CheckRateLimitAsync(
            Guid tenantId,
            int limit,
            int periodInSeconds)
        {
            var key = $"ratelimit:{tenantId}";

            // Atomic increment
            var current = await _db.StringIncrementAsync(key);

            // İlk request ise TTL set et
            if (current == 1)
            {
                await _db.KeyExpireAsync(
                    key,
                    TimeSpan.FromSeconds(periodInSeconds));
            }

            return current <= limit;
        }

        // --------------------------------------------------
        // 4️ Quota check (monthly / fixed window)
        // --------------------------------------------------
        public async Task<bool> CheckQuotaAsync(Guid tenantId, int limit)
        {
            var now = DateTime.UtcNow;
            var key = $"quota:{tenantId}:{now.Year}{now.Month:00}";

            var value = await _db.StringGetAsync(key);

            if (!value.HasValue)
                return true;

            return (int)value < limit;
        }
    }
}
