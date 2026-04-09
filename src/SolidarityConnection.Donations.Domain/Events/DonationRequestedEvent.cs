namespace SolidarityConnection.Donations.Domain.Events
{
    public record DonationRequestedEvent(
        int DonorCode,
        int CampaignCode,
    decimal Amount,
    DateTimeOffset RequestedAt,
    Guid CorrelationId);
}
