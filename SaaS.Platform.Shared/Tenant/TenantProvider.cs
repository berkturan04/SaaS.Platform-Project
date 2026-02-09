using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Shared.Tenant
{
    public class TenantProvider:ITenantProvider
    {
        public Guid TenantId { get; private set; }
        public void SetTenant(Guid tenantId)
        {
            TenantId = TenantId;
        }
    }
}
