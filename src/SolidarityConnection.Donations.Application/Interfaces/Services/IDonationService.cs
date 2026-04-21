using SolidarityConnection.Donations.Application.DTOs;
using SolidarityConnection.Donations.Domain.Events;

namespace SolidarityConnection.Donations.Application.Interfaces.Services
{
    public interface IDonationService
    {
        Task ProcessAsync(DonationRequestedEvent message, CancellationToken cancellationToken);
    }
}
