using SolidarityConnection.Donations.Application.DTOs;
using SolidarityConnection.Donations.Application.Interfaces.Publishers;
using SolidarityConnection.Donations.Domain.Events;
using SolidarityConnection.Donations.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;

public class DonationRequestedEventPublisher : IDonationRequestedEventPublisher
{
    private readonly IServiceBusPublisher _busPublisher;
    private readonly ILogger<DonationRequestedEventPublisher> _logger;
    private const string DonationRequestedTopic = "donations-requested";

    public DonationRequestedEventPublisher(IServiceBusPublisher busPublisher, ILogger<DonationRequestedEventPublisher> logger)
    {
        _busPublisher = busPublisher;
        _logger = logger;
    }

    public async Task PublishAsync(DonationRequestDto dto)
    {
        var donationRequestedEvent = new DonationRequestedEvent(
            dto.DonorCode,
            dto.CampaignCode,
            dto.Amount,
            dto.RequestedAt,
            Guid.NewGuid());

        try
        {
            await _busPublisher.PublishAsync(donationRequestedEvent, DonationRequestedTopic);
        }
        catch (Exception e)
        {
            _logger.LogError(
                "Error publishing DonationRequestedEvent: DonorCode={DonorCode}, CampaignCode={CampaignCode}. Message={Message}",
                dto.DonorCode,
                dto.CampaignCode,
                e.Message);
        }
    }
}
