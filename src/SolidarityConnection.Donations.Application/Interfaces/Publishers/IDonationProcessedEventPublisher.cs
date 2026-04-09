using SolidarityConnection.Donations.Domain.Entities;
using SolidarityConnection.Donations.Domain.Events;

namespace SolidarityConnection.Donations.Application.Interfaces.Publishers
{
    public interface IDonationProcessedEventPublisher
    {
        Task PublishAsync(DonationRequestedEvent requestEvent, Donation? donation, bool success);
    }
}
