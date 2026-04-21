namespace SolidarityConnection.Donations.Domain.Entities
{
    public class Donation
    {
        public Guid Id { get; set; }
        public Guid DonorId { get; set; }
        public Guid CampaignId { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset RequestedAt { get; set; }
        public DateTimeOffset? ProcessedAt { get; set; }
        public int Status { get; set; } = 1;
        public int Attempts { get; set; }
        public DateTimeOffset? LastRetryAt { get; set; }
        public Guid CorrelationId { get; set; }
        public string? FailureReason { get; set; }

        public CampaignReference? Campaign { get; set; }
        public DonorReference? Donor { get; set; }
    }
}
