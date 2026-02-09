using SaaS.Platform.Application.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Infrastructure.Payments
{
    public sealed class FakePaymentGateway : IPaymentGateway
    {
        public Task ChargeAsync(Guid invoiceId, CancellationToken ct)
        {
            // simulate flaky external system
            if (Random.Shared.Next(1, 5) == 1)
                throw new Exception("Payment failed");

            return Task.CompletedTask;
        }
    }
}
