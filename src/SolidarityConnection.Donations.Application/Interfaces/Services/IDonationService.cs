using SolidarityConnection.Donations.Application.DTOs;
using SolidarityConnection.Donations.Domain.Entities;
using SolidarityConnection.Donations.Domain.Events;

namespace SolidarityConnection.Donations.Application.Interfaces.Services
{
    public interface IDonationService
    {
        Task<Donation?> GetByCampaignAndDonorCodeAsync(int campaignCode, int donorCode);
        Task CreateDonationRequest(DonationRequestDto dto);
        Task ProcessAsync(DonationRequestedEvent message, CancellationToken cancellationToken);
    }
}
