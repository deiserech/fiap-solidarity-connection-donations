namespace SolidarityConnection.Donations.Application.DTOs
{
    public class DonationResultDto
    {
        public Guid DonationId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset? ProcessedAt { get; set; }
    }
}
