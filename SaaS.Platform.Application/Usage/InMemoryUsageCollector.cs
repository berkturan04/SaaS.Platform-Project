using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Application.Usage
{
    public class InMemoryUsageCollector : IUsageCollector
    {
        private readonly ConcurrentDictionary<Guid, int> _usage = new();

        public void Increment(Guid tenantId) 
        {
            _usage.AddOrUpdate(tenantId,1,(_,current)=> current +1);
        }
        public Dictionary<Guid, int> Drain() 
        {
            var snapshot = new Dictionary<Guid, int>(_usage);
            _usage.Clear();
            return snapshot;
        }
    }
}
