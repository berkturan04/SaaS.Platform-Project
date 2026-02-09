using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Application.Usage
{
    public interface IUsageTrackingService
    {
        Task TrackAsync();
        Task IncrementAsync(Guid tenantId, int year, int month);
        Task<bool> CheckRateLimitAsync(Guid tenantId, int limit, int periodInSeconds);
        Task<bool> CheckQuotaAsync(Guid tenantId, int limit);
    }
}
