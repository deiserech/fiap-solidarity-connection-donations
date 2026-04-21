using SolidarityConnection.Donations.Application.DTOs;
using SolidarityConnection.Donations.Domain.Events;

namespace SolidarityConnection.Donations.Application.Interfaces.Services
{
    public interface IDonationService
    {
        Task CreateDonationRequest(DonationRequestDto dto);
        Task ProcessAsync(DonationRequestedEvent message, CancellationToken cancellationToken);
    }
}
