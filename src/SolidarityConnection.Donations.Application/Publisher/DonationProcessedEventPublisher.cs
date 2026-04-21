using SolidarityConnection.Donations.Application.Interfaces.Publishers;
using SolidarityConnection.Donations.Domain.Entities;
using SolidarityConnection.Donations.Domain.Events;
using SolidarityConnection.Donations.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;

public class DonationProcessedEventPublisher : IDonationProcessedEventPublisher
{
    private readonly IServiceBusPublisher _busPublisher;
    private readonly ILogger<DonationProcessedEventPublisher> _logger;
    private const string DonationProcessedTopic = "donation-processed";

    public DonationProcessedEventPublisher(IServiceBusPublisher busPublisher, ILogger<DonationProcessedEventPublisher> logger)
    {
        _busPublisher = busPublisher;
        _logger = logger;
    }

    public async Task PublishAsync(DonationRequestedEvent requestEvent, Donation? donation, bool success)
    {
        var donationId = donation?.Id ?? requestEvent.DonationId;
        var campaignId = donation?.CampaignId ?? requestEvent.CampaignId;
        var donationAmount = donation?.Amount ?? requestEvent.DonationAmount;
        var processedAt = donation?.ProcessedAt ?? DateTimeOffset.UtcNow;
        var status = success ? "Processed" : "Failed";
        var errorMessage = success ? null : donation?.FailureReason ?? "Donation processing failed";

        var donationProcessedEvent = new DonationProcessedEvent(
            donationId,
            campaignId,
            donationAmount,
            status,
            processedAt,
            errorMessage);

        try
        {
            await _busPublisher.PublishAsync(donationProcessedEvent, DonationProcessedTopic);
        }
        catch (Exception e)
        {
            _logger.LogError(
                "Error publishing DonationProcessedEvent: DonationId={DonationId}, CampaignId={CampaignId}. Message={Message}",
                donationId,
                campaignId,
                e.Message);
        }
    }
}
