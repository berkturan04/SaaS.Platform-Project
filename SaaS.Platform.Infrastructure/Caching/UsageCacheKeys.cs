using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Infrastructure.Caching
{
    public static class UsageCacheKeys
    {
        public static string Usage(Guid tenantId, int year, int month)
            => $"usage:{tenantId}:{year}{month:00}";
    }
}
