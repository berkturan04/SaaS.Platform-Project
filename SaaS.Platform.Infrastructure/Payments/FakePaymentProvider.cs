using SaaS.Platform.Application.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Infrastructure.Payments
{
    public class FakePaymentProvider : IPaymentProvider
    {
        public Task<bool> ChargeAsync(
            Guid invoiceId,
            decimal amount,
            Guid tenantId)
        {
            // %90 success
            return Task.FromResult(
                Random.Shared.Next(1, 10) != 1);
        }
    }
}
