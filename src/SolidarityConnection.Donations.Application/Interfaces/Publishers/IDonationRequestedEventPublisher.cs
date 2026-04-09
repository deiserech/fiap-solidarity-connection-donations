using SolidarityConnection.Donations.Application.DTOs;

namespace SolidarityConnection.Donations.Application.Interfaces.Publishers
{
    public interface IDonationRequestedEventPublisher
    {
        Task PublishAsync(DonationRequestDto request);
    }
}
