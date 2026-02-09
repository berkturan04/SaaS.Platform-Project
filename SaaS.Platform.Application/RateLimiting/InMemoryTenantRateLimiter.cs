using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Application.RateLimiting
{
    public class InMemoryTenantRateLimiter : ITenantRateLimiter
    {
        private readonly ConcurrentDictionary<Guid, Counter> _counters = new();
        private const int LIMIT = 100;

        public bool AllowRequest(Guid tenantId) 
        {
            var counter = _counters.GetOrAdd(tenantId, _ => new Counter { WindowStart = DateTime.UtcNow }
            );
            lock (counter) 
            {
                if(DateTime.UtcNow - counter.WindowStart > TimeSpan.FromMinutes(1)) 
                {
                    counter.WindowStart = DateTime.UtcNow;
                    counter.Count = 0;
                }
                counter.Count++;
                return counter.Count <= LIMIT;
            }
        }
        private class Counter 
        {
            public DateTime WindowStart;
            public int Count;
        }
    }
}
