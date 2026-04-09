using SolidarityConnection.Donations.Application.Interfaces.Services;
using SolidarityConnection.Donations.Domain.Entities;
using SolidarityConnection.Donations.Domain.Events;
using SolidarityConnection.Donations.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace SolidarityConnection.Donations.Application.Services
{
    public class CampaignReferenceService : ICampaignReferenceService
    {
        private readonly ICampaignReferenceRepository _repo;
        private readonly ILogger<CampaignReferenceService> _logger;

        public CampaignReferenceService(ICampaignReferenceRepository repo, ILogger<CampaignReferenceService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task ProcessAsync(CampaignUpsertedEvent message, CancellationToken cancellationToken = default)
        {
            var campaign = await _repo.GetByCodeAsync(message.Code);
            if (campaign is null && message.RemovedAt != null)
            {
                _logger.LogWarning("Campaign is removed: {CampaignCode}", message.Code);
                return;
            }

            if (campaign?.UpdatedAt > message.UpdatedAt)
            {
                _logger.LogWarning("Message is older then saved data: {CampaignCode}", message.Code);
                return;
            }

            if (campaign is null)
            {
                campaign = new CampaignReference(
                    message.Code,
                    message.Title,
                    message.GoalAmount,
                    message.Status,
                    message.UpdatedAt,
                    message.RemovedAt == null
                );

                await _repo.CreateAsync(campaign);
                _logger.LogInformation("Campaign created: {CampaignCode}", message.Code);
                return;
            }

            campaign.Code = message.Code;
            campaign.Title = message.Title;
            campaign.GoalAmount = message.GoalAmount;
            campaign.Status = message.Status;
            campaign.UpdatedAt = message.UpdatedAt;
            campaign.IsActive = message.RemovedAt == null;

            await _repo.UpdateAsync(campaign);
            _logger.LogInformation("Campaign updated: {CampaignCode}", message.Code);
        }
    }
}