using SolidarityConnection.Donations.Application.Interfaces.Services;
using SolidarityConnection.Donations.Domain.Events;

namespace SolidarityConnection.Donations.Api.BackgroundServices.Donations
{
    public class DonationRequestedMessageHandler : IDonationRequestedMessageHandler
    {
        private readonly IDonationService _donationService;
        private readonly ILogger<DonationRequestedMessageHandler> _logger;

        public DonationRequestedMessageHandler(IDonationService DonationService, ILogger<DonationRequestedMessageHandler> logger)
        {
            _donationService = DonationService;
            _logger = logger;
        }

        public async Task HandleAsync(DonationRequestedEvent message, CancellationToken cancellationToken)
        {
            try
            {
                await _donationService.ProcessAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling donation requested message");
                throw;
            }
        }
    }
}
