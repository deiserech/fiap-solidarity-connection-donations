namespace SolidarityConnection.Donations.Domain.Events
{
    public record DonorUpsertedEvent(
        int Code,
        string Email,
        DateTimeOffset UpdatedAt,
        DateTimeOffset? RemovedAt);
}
