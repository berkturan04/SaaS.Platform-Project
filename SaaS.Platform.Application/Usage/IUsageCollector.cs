using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Application.Usage
{
    public interface IUsageCollector
    {
        void Increment(Guid tenantId);
        Dictionary<Guid, int> Drain();

    }
}
