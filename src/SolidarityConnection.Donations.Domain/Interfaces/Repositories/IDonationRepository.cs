using SolidarityConnection.Donations.Domain.Entities;

namespace SolidarityConnection.Donations.Application.Interfaces.Repositories
{
    public interface IDonationRepository
    {
        Task<Donation?> GetByCampaignAndDonorCodeAsync(int campaignCode, int donorCode);
        Task<Donation> AddAsync(Donation donation);
        Task<IEnumerable<Donation>> GetAllAsync();
        Task<Donation?> GetByIdAsync(Guid id);
        Task<Donation> UpdateAsync(Donation donation);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}
