using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Infrastructure.Payments
{
    public class WebhookEvent
    {
        public Guid Id { get; set; }

        public string ProviderEventId { get; set; } = null!;

        public string EventType { get; set; } = null!;

        public string Payload { get; set; } = null!;

        public DateTime ReceivedAt { get; set; }

        public bool Processed { get; set; }
    }
}
