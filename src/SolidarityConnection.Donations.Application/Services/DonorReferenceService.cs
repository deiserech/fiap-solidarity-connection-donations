using SolidarityConnection.Donations.Domain.Entities;
using SolidarityConnection.Donations.Domain.Events;
using SolidarityConnection.Donations.Domain.Interfaces.Repositories;
using SolidarityConnection.Donations.Shared.Tracing;
using SolidarityConnection.Donations.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace SolidarityConnection.Donations.Application.Services
{
    public class DonorReferenceService : IDonorReferenceService
    {
        private readonly IDonorReferenceRepository _repo;
        private readonly ILogger<DonorReferenceService> _logger;

        public DonorReferenceService(IDonorReferenceRepository repo, ILogger<DonorReferenceService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task ProcessAsync(DonorUpsertedEvent message, CancellationToken cancellationToken = default)
        {
            var donor = await _repo.GetByCodeAsync(message.Code);
            if (donor is null && message.RemovedAt != null)
            {
                _logger.LogWarning("Donor is removed: {DonorCode}", message.Code);
                return;
            }

            if (donor?.UpdatedAt > message.UpdatedAt)
            {
                _logger.LogWarning("Message is older then saved data: {DonorCode}", message.Code);
                return;
            }

            if (donor is null)
            {
                donor = new DonorReference(message.Code,
                                message.Email,
                                message.UpdatedAt,
                                message.RemovedAt != null);

                await _repo.CreateAsync(donor);
                _logger.LogInformation("Donor created: {DonorCode}", message.Code);
                return;
            }

            donor.Code = message.Code;
            donor.Email = message.Email;
            donor.UpdatedAt = message.UpdatedAt;
            donor.IsActive = message.RemovedAt != null;

            await _repo.UpdateAsync(donor);
            _logger.LogInformation("Donor updated: {DonorCode}", message.Code);
            return;
        }

        public async Task<DonorReference?> GetByIdAsync(Guid id)
        {
            using var activity = Tracing.ActivitySource.StartActivity($"{nameof(DonorReferenceService)}.GetByIdAsync");
            _logger.LogInformation("Fetching donor by Id: {Id}", id);
            return await _repo.GetByIdAsync(id);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            using var activity = Tracing.ActivitySource.StartActivity($"{nameof(DonorReferenceService)}.ExistsAsync");
            _logger.LogInformation("Checking donor existence by Id: {Id}", id);
            return await _repo.ExistsAsync(id);
        }

        public async Task<DonorReference> CreateDonorAsync(DonorUpsertedEvent donorEvent)
        {
            using var activity = Tracing.ActivitySource.StartActivity($"{nameof(DonorReferenceService)}.CreateDonorAsync");
            _logger.LogInformation("Creating donor: {Email}", donorEvent.Email);
            var donor = new DonorReference(donorEvent.Code,
                                           donorEvent.Email,
                                           donorEvent.UpdatedAt,
                                           donorEvent.RemovedAt != null);

            var created = await _repo.CreateAsync(donor);
            _logger.LogInformation("Donor created successfully: {Email}", donorEvent.Email);
            return created;
        }
    }
}