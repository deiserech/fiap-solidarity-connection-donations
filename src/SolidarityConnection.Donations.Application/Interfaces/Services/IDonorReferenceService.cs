
using SolidarityConnection.Donations.Domain.Entities;
using SolidarityConnection.Donations.Domain.Events;

namespace SolidarityConnection.Donations.Application.Interfaces.Services
{
    public interface IDonorReferenceService
    {
        Task ProcessAsync(DonorUpsertedEvent message, CancellationToken cancellationToken = default);
        Task<DonorReference?> GetByIdAsync(Guid id);
        Task<DonorReference> CreateDonorAsync(DonorUpsertedEvent donor);
        Task<bool> ExistsAsync(Guid id);
    }
}
