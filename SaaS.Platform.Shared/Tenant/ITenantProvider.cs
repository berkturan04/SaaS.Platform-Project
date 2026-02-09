using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Shared.Tenant
{
    public interface ITenantProvider
    {
        Guid TenantId { get; }
        void SetTenant(Guid tenantId);
    }
}
