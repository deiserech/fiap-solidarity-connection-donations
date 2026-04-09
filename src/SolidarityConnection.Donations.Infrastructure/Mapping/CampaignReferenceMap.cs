using SolidarityConnection.Donations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SolidarityConnection.Donations.Infrastructure.Mappings
{
    public class CampaignReferenceMap : IEntityTypeConfiguration<CampaignReference>
    {
        public void Configure(EntityTypeBuilder<CampaignReference> builder)
        {
            builder.HasKey(g => g.Id);

            builder.Property(g => g.Code)
                .IsRequired();

            builder.Property(g => g.Title)
                .HasMaxLength(200);

            builder.Property(g => g.GoalAmount)
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(g => g.Status)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(g => g.UpdatedAt)
                .IsRequired();

            builder.Property(g => g.IsActive)
                .IsRequired();

            builder.HasIndex(g => g.Code).IsUnique();

            builder.ToTable("CampaignReferences");
        }
    }
}
