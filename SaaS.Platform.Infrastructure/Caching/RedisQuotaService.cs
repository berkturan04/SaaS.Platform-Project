using Microsoft.Extensions.Caching.Distributed;
using SaaS.Platform.Application.Interfaces;
using SaaS.Platform.Shared.Tenant;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Infrastructure.Caching
{
    public sealed class RedisQuotaService : IQuotaService
    {
        private readonly IDatabase _redis;
        private readonly ITenantProvider _tenantProvider;

        private const int MonthlyQuota = 100_000;

        public RedisQuotaService(
            IConnectionMultiplexer redis,
            ITenantProvider tenantProvider)
        {
            _redis = redis.GetDatabase();
            _tenantProvider = tenantProvider;
        }

        public async Task<bool> CheckQuotaAsync()
        {
            var tenantId = _tenantProvider.TenantId;
            var now = DateTime.UtcNow;

            var key = $"quota:{tenantId}:{now.Year}{now.Month:00}";

            var current = await _redis.StringGetAsync(key);

            if (current.HasValue && (int)current >= MonthlyQuota)
                return false;

            return true;
        }
    }
}
