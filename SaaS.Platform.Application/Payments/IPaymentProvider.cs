using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Application.Payments
{
    public interface IPaymentProvider
    {
        Task<bool> ChargeAsync(
            Guid invoiceId,
            decimal amount,
            Guid tenantId);
    }
}
