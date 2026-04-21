using SolidarityConnection.Donations.Application.DTOs;
using SolidarityConnection.Donations.Application.Interfaces.Publishers;
using SolidarityConnection.Donations.Domain.Events;
using SolidarityConnection.Donations.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;

public class DonationRequestedEventPublisher : IDonationRequestedEventPublisher
{
    private readonly IServiceBusPublisher _busPublisher;
    private readonly ILogger<DonationRequestedEventPublisher> _logger;
    private const string DonationRequestedTopic = "donation-requested";

    public DonationRequestedEventPublisher(IServiceBusPublisher busPublisher, ILogger<DonationRequestedEventPublisher> logger)
    {
        _busPublisher = busPublisher;
        _logger = logger;
    }

    public async Task PublishAsync(DonationRequestDto dto)
    {
        var donorId = dto.DonorId == Guid.Empty ? Guid.NewGuid() : dto.DonorId;
        var campaignId = dto.CampaignId == Guid.Empty ? Guid.NewGuid() : dto.CampaignId;

        var donationRequestedEvent = new DonationRequestedEvent(
            Guid.NewGuid(),
            campaignId,
            donorId,
            dto.Amount,
            dto.RequestedAt);

        try
        {
            await _busPublisher.PublishAsync(donationRequestedEvent, DonationRequestedTopic);
        }
        catch (Exception e)
        {
            _logger.LogError(
                "Error publishing DonationRequestedEvent: DonorId={DonorId}, CampaignId={CampaignId}. Message={Message}",
                donorId,
                campaignId,
                e.Message);
        }
    }
}
