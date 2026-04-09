using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SolidarityConnection.Donations.Domain.Entities;

namespace SolidarityConnection.Donations.Application.DTOs
{
    public class DonationRequestDto
    {
        [Required]
        public int DonorCode { get; set; }

        [Required]
        public int CampaignCode { get; set; }

        [Required]
        [Range(0.01, 1000.00)]
        public decimal Amount { get; set; }

        [Required]
        public DateTimeOffset RequestedAt { get; set; }

        [JsonIgnore]
        public Guid DonorId { get; set; }

        [JsonIgnore]
        public Guid CampaignId { get; set; }

        public static Donation ToEntity(DonationRequestDto dto)
        {
            return new()
            {
                DonorId = dto.DonorId,
                CampaignId = dto.CampaignId,
                Amount = dto.Amount,
                RequestedAt = dto.RequestedAt,
                CorrelationId = Guid.NewGuid(),
                Status = 1
            };
        }

        public static DonationRequestDto FromEntity(Donation entity)
        {
            return new() {
                DonorCode = entity.Donor!.Code,
                CampaignCode = entity.Campaign!.Code,
                RequestedAt = entity.RequestedAt,
                Amount = entity.Amount
            };
        }

    }
}
