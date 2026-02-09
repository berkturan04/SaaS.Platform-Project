using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Application.Interfaces
{
    public interface IQuotaService
    {
        Task<bool> CheckQuotaAsync();
    }
}
