using SolidarityConnection.Donations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SolidarityConnection.Donations.Infrastructure.Mappings
{
    public class DonorReferenceMap : IEntityTypeConfiguration<DonorReference>
    {
        public void Configure(EntityTypeBuilder<DonorReference> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Code)
                .IsRequired();

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(u => u.UpdatedAt)
                .IsRequired();

            builder.Property(u => u.IsActive)
                .IsRequired();

            builder.HasIndex(u => u.Code).IsUnique();
            builder.HasIndex(u => u.Email).IsUnique();

            builder.ToTable("DonorReferences");
        }
    }
}
