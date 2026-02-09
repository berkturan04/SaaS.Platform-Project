using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Domain.Entities
{
    public class TenantUsage
    {
        public Guid TenantId { get; set; }
        public DateTime Period { get; set; }
        public int RequestCount { get; set; }
    }
}
