using SolidarityConnection.Donations.Domain.Entities;

namespace SolidarityConnection.Donations.Domain.Interfaces.Repositories
{
    public interface IDonorReferenceRepository
    {
        Task<DonorReference?> GetByIdAsync(Guid id);
        Task<DonorReference?> GetByCodeAsync(int code);
        Task<DonorReference?> GetByEmailAsync(string email);
        Task<IEnumerable<DonorReference>> GetAllAsync();
        Task<DonorReference> CreateAsync(DonorReference donor);
        Task<DonorReference> UpdateAsync(DonorReference donor);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> EmailExistsAsync(string email);
    }
}
