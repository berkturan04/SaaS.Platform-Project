using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Domain.Entities
{
    public class UsageCounter : BaseTenantEntity
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Count { get; private set; }

        public void Add(int value)
        {
            Count += value;
        }
    }
}
