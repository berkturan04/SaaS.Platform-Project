using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Domain.Events
{
    public record ResourceCreatedEvent(
    Guid ResourceId,
    Guid TenantId,
    string Name
);
}
