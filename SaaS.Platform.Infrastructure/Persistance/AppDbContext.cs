using Microsoft.EntityFrameworkCore;
using SaaS.Platform.Domain.Entities;
using SaaS.Platform.Infrastructure.Persistance.Outbox;
using SaaS.Platform.Shared.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Infrastructure.Persistance
{
    public class AppDbContext : DbContext
    {
        private readonly ITenantProvider _tenantProvider;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            ITenantProvider tenantProvider)
            : base(options)
        {
            _tenantProvider = tenantProvider;
        }

        public DbSet<Resource> Resources => Set<Resource>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Resource>()
                .HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId);
            modelBuilder.Entity<TenantInvoice>()
                .HasIndex(x => new { x.TenantId, x.Year, x.Month })
                .IsUnique();
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseTenantEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(AppDbContext)
                        .GetMethod(nameof(ApplyFilter),
                            BindingFlags.NonPublic | BindingFlags.Static)!
                        .MakeGenericMethod(entityType.ClrType);

                    method.Invoke(null, new object[] { modelBuilder, this });
                }
            }
            base.OnModelCreating(modelBuilder);
        }

        private static void ApplyFilter<TEntity>(ModelBuilder modelBuilder, AppDbContext context) where TEntity : BaseTenantEntity 
        {
            modelBuilder.Entity<TEntity>()
                .HasQueryFilter(e => !e.IsDeleted && e.TenantId == context._tenantProvider.TenantId);

        } 
    }
}
