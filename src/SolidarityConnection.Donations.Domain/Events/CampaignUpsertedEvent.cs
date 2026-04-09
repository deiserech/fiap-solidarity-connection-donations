namespace SolidarityConnection.Donations.Domain.Events
{
    public record CampaignUpsertedEvent(
        int Code,
        string Title,
        decimal GoalAmount,
        string Status,
        DateTimeOffset UpdatedAt,
        DateTimeOffset? RemovedAt);
}
