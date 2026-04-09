using SolidarityConnection.Donations.Domain.Entities;

using SolidarityConnection.Donations.Infrastructure.Mappings;
using Microsoft.EntityFrameworkCore;

namespace SolidarityConnection.Donations.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<DonorReference> DonorReferences { get; set; }
        public DbSet<CampaignReference> CampaignReferences { get; set; }
        public DbSet<Donation> Donations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new DonorReferenceMap());
            modelBuilder.ApplyConfiguration(new CampaignReferenceMap());
            modelBuilder.ApplyConfiguration(new DonationMap());
        }
    }
}
