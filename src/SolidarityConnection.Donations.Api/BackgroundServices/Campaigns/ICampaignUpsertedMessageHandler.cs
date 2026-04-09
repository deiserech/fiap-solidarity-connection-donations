using SolidarityConnection.Donations.Domain.Events;

namespace SolidarityConnection.Donations.Api.BackgroundServices.Campaigns
{
    public interface ICampaignUpsertedMessageHandler
    {
        Task HandleAsync(CampaignUpsertedEvent message, CancellationToken cancellationToken);
    }
}
