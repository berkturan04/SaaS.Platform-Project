using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Domain.Entities
{
    public class Resource : BaseTenantEntity
    {
        public string Name { get; set; } = default!;
    }
}
