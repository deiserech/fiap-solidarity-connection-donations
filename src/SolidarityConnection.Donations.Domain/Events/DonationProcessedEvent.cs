namespace SolidarityConnection.Donations.Domain.Events
{
    public record DonationProcessedEvent(
    Guid? DonationId,
        int DonorCode,
        int CampaignCode,
        DateTimeOffset? ProcessedAt,
    bool Success,
    decimal Amount);
}
