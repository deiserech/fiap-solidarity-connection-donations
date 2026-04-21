using SolidarityConnection.Donations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SolidarityConnection.Donations.Infrastructure.Mappings
{
    public class DonationMap : IEntityTypeConfiguration<Donation>
    {
        public void Configure(EntityTypeBuilder<Donation> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Amount)
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(d => d.RequestedAt)
                .IsRequired();

            builder.Property(d => d.ProcessedAt);

            builder.Property(d => d.Status)
                .IsRequired();

            builder.Property(d => d.Attempts)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(d => d.LastRetryAt);

            builder.Property(d => d.CorrelationId)
                .IsRequired();

            builder.Property(d => d.FailureReason)
                .HasMaxLength(500);

            builder.HasOne(d => d.Donor)
                .WithMany()
                .HasForeignKey(d => d.DonorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Campaign)
                .WithMany()
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("Donations");
        }
    }
}
