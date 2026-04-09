using SolidarityConnection.Donations.Domain.Entities;

namespace SolidarityConnection.Donations.Domain.Interfaces.Repositories
{
    public interface ICampaignReferenceRepository
    {
        Task<CampaignReference?> GetByIdAsync(Guid id);
        Task<CampaignReference?> GetByCodeAsync(int code);
        Task<CampaignReference> CreateAsync(CampaignReference campaign);
        Task<CampaignReference> UpdateAsync(CampaignReference campaign);
        Task<bool> ExistsAsync(Guid id);
    }
}
