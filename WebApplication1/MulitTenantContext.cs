using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1
{
    public class MulitTenantContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {

        public static Guid Tenant1Id = Guid.Parse("51aab199-1482-4f0d-8ff1-5ca0e7bc525a");
        public static Guid Tenant2Id = Guid.Parse("ae4e21fa-57cb-4733-b971-fdd14c4c667e");

        public DbSet<Person> People { get; set; }

        private Tenant _tenant;

        //public MultitenantContext(ITenantProvider tenantProvider, ILogger<MultitenantDbContext> logger)
        //{
        //    _tenant = tenantProvider.GetTenant();
        //}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Comment out for real application
            //optionsBuilder.UseSqlServer(_tenant.DatabaseConnectionString);

            // Comment in for real applications
            //optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());

            base.OnConfiguring(optionsBuilder);

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Person>().HasQueryFilter(p => p.TenantId == _tenant.Id);
        }

        public void AddSampleData()
        {
            People.Add(new Person
            {
                Id = Guid.Parse("79865406-e01b-422f-bd09-92e116a0664a"),
                TenantId = Tenant1Id,
                FirstName = "Gunnar",
                LastName = "Peipman"
            });

            People.Add(new Person
            {
                Id = Guid.Parse("d5674750-7f6b-43b9-b91b-d27b7ac13572"),
                TenantId = Tenant2Id,
                FirstName = "John",
                LastName = "Doe"
            });

            People.Add(new Person
            {
                Id = Guid.Parse("e41446f9-c779-4ff6-b3e5-752a3dad97bb"),
                TenantId = Tenant1Id,
                FirstName = "Mary",
                LastName = "Jones"
            });

            SaveChanges();
        }
    }
}
