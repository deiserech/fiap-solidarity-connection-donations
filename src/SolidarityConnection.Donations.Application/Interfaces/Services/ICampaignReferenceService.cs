using SolidarityConnection.Donations.Domain.Events;

namespace SolidarityConnection.Donations.Application.Interfaces.Services
{
    public interface ICampaignReferenceService
    {
        Task ProcessAsync(CampaignUpsertedEvent message, CancellationToken cancellationToken = default);
    }
}
