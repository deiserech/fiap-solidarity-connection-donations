using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

    }
}
