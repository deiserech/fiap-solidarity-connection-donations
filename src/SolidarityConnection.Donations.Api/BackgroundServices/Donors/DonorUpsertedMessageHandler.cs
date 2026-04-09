using SolidarityConnection.Donations.Domain.Events;
using SolidarityConnection.Donations.Application.Interfaces.Services;

namespace SolidarityConnection.Donations.Api.BackgroundServices.Donors
{
    public class DonorUpsertedMessageHandler : IDonorUpsertedMessageHandler
    {
        private readonly IDonorReferenceService _donorService;
        private readonly ILogger<DonorUpsertedMessageHandler> _logger;

        public DonorUpsertedMessageHandler(IDonorReferenceService DonorReferenceService, ILogger<DonorUpsertedMessageHandler> logger)
        {
            _donorService = DonorReferenceService;
            _logger = logger;
        }

        public async Task HandleAsync(DonorUpsertedEvent message, CancellationToken cancellationToken)
        {
            try
            {
                await _donorService.ProcessAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling donor message");
                throw;
            }
        }
    }
}
