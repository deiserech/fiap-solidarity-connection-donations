using SolidarityConnection.Donations.Domain.Events;

namespace SolidarityConnection.Donations.Api.BackgroundServices.Donations
{
    public interface IDonationRequestedMessageHandler
    {
        Task HandleAsync(DonationRequestedEvent message, CancellationToken cancellationToken);
    }
}
