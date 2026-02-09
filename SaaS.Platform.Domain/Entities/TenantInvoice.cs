using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Domain.Entities
{
    public class TenantInvoice : BaseTenantEntity
    {
        public int Year { get; private set; }
        public int Month { get; private set; }
        public int Usage { get; private set; }
        public decimal Amount { get; private set; }

        public bool IsPaid { get; private set; }
        public DateTime? PaidAt { get; private set; }

        protected TenantInvoice() { }

        public TenantInvoice(
            Guid tenantId,
            int year,
            int month,
            int usage,
            decimal pricePerUnit)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            Year = year;
            Month = month;
            Usage = usage;
            Amount = usage * pricePerUnit;
            IsPaid = false;
        }

        public void MarkPaid()
        {
            IsPaid = true;
            PaidAt = DateTime.UtcNow;
        }
    }
}
