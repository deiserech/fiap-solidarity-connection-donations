using SolidarityConnection.Donations.Application.Interfaces.Services;
using SolidarityConnection.Donations.Domain.Events;

namespace SolidarityConnection.Donations.Api.BackgroundServices.Campaigns
{
    public class CampaignUpsertedMessageHandler : ICampaignUpsertedMessageHandler
    {
        private readonly ICampaignReferenceService _campaignService;
        private readonly ILogger<CampaignUpsertedMessageHandler> _logger;

        public CampaignUpsertedMessageHandler(ICampaignReferenceService campaignService, ILogger<CampaignUpsertedMessageHandler> logger)
        {
            _campaignService = campaignService;
            _logger = logger;
        }

        public async Task HandleAsync(CampaignUpsertedEvent message, CancellationToken cancellationToken)
        {
            try
            {
                await _campaignService.ProcessAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling campaign message");
                throw;
            }
        }
    }
}
