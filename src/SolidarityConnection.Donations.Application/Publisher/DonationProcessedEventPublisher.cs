using SolidarityConnection.Donations.Application.Interfaces.Publishers;
using SolidarityConnection.Donations.Domain.Entities;
using SolidarityConnection.Donations.Domain.Events;
using SolidarityConnection.Donations.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;

public class DonationProcessedEventPublisher : IDonationProcessedEventPublisher
{
    private readonly IServiceBusPublisher _busPublisher;
    private readonly ILogger<DonationProcessedEventPublisher> _logger;
    private const string DonationProcessedTopic = "donations-processed";

    public DonationProcessedEventPublisher(IServiceBusPublisher busPublisher, ILogger<DonationProcessedEventPublisher> logger)
    {
        _busPublisher = busPublisher;
        _logger = logger;
    }

    public async Task PublishAsync(DonationRequestedEvent requestEvent, Donation? donation, bool success)
    {
        var donationProcessedEvent = new DonationProcessedEvent(
            donation?.Id,
            donation?.Donor?.Code ?? requestEvent.DonorCode,
            donation?.Campaign?.Code ?? requestEvent.CampaignCode,
            donation?.ProcessedAt ?? requestEvent.RequestedAt,
            success,
            donation?.Amount ?? requestEvent.Amount);

        try
        {
            await _busPublisher.PublishAsync(donationProcessedEvent, DonationProcessedTopic);
        }
        catch (Exception e)
        {
            _logger.LogError(
                "Error publishing DonationProcessedEvent: DonorCode={DonorCode}, CampaignCode={CampaignCode}. Message={Message}",
                requestEvent.DonorCode,
                requestEvent.CampaignCode,
                e.Message);
        }
    }
}
