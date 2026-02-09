using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Domain.Entities
{
    public class BaseTenantEntity
    {
        public Guid Id { get; set; }

        public Guid TenantId { get; set; }

        public bool IsDeleted { get; private set; }

        public DateTime? DeletedAt { get; private set; }

        protected void SoftDelete()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }

        public void Delete() 
        {
            if (IsDeleted)
                return;
            SoftDelete();
        }
    }
}
