using SolidarityConnection.Donations.Domain.Events;

namespace SolidarityConnection.Donations.Api.BackgroundServices.Donors
{
    public interface IDonorUpsertedMessageHandler
    {
        Task HandleAsync(DonorUpsertedEvent message, CancellationToken cancellationToken);
    }
}
