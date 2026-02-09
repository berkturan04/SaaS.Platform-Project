using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Infrastructure.Persistance.Outbox
{
    public class PaymentOutboxMessage
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int RetryCount { get; set; }
        public bool Processed { get; set; }
    }
}
