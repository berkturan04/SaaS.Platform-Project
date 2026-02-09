using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Infrastructure.Persistance.Outbox
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public DateTime OccurredOnUtc { get; set; }

        public string Type { get; set; } = null!;
        public string Payload { get; set; } = null!;

        public DateTime? ProcessedOnUtc { get; set; }
    }
}
