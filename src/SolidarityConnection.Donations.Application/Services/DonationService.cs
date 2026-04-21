using SolidarityConnection.Donations.Application.DTOs;
using SolidarityConnection.Donations.Application.Interfaces.Publishers;
using SolidarityConnection.Donations.Application.Interfaces.Repositories;
using SolidarityConnection.Donations.Application.Interfaces.Services;
using SolidarityConnection.Donations.Domain.Entities;
using SolidarityConnection.Donations.Domain.Events;
using Microsoft.Extensions.Logging;

namespace SolidarityConnection.Donations.Application.Services
{
    public class DonationService : IDonationService
    {
        private const int StatusReceived = 1;
        private const int StatusProcessing = 2;
        private const int StatusProcessed = 3;
        private const int StatusFailed = 4;

        private readonly IDonationRepository _donationRepository;
        private readonly IDonationRequestedEventPublisher _donationRequestedEventPublisher;
        private readonly IDonationProcessedEventPublisher _donationProcessedEventPublisher;
        private readonly ILogger<DonationService> _logger;

        public DonationService(
            IDonationRepository donationRepository,
            IDonationRequestedEventPublisher donationRequestedEventPublisher,
            IDonationProcessedEventPublisher donationProcessedEventPublisher,
            ILogger<DonationService> logger)
        {
            _donationRepository = donationRepository;
            _donationRequestedEventPublisher = donationRequestedEventPublisher;
            _donationProcessedEventPublisher = donationProcessedEventPublisher;
            _logger = logger;
        }

        public async Task CreateDonationRequest(DonationRequestDto dto)
        {
            await _donationRequestedEventPublisher.PublishAsync(dto);
        }

        public async Task ProcessAsync(DonationRequestedEvent message, CancellationToken cancellationToken)
        {
            if (await _donationRepository.ExistsAsync(message.DonationId))
            {
                _logger.LogInformation("Skipping duplicated donation message. DonationId={DonationId}", message.DonationId);
                return;
            }

            bool success = false;

            var donation = new Donation
            {
                Id = message.DonationId,
                DonorId = message.DonorId,
                CampaignId = message.CampaignId,
                Amount = message.DonationAmount,
                RequestedAt = message.RequestedAt,
                CorrelationId = message.DonationId,
                Status = StatusReceived
            };

            Donation? createdDonation = null;
            try
            {
                donation.Status = StatusProcessing;
                success = await ProcessDonation(message, cancellationToken);

                donation.Status = success ? StatusProcessed : StatusFailed;
                donation.ProcessedAt = DateTimeOffset.UtcNow;
                donation.FailureReason = success ? null : "Donation validation failed.";

                createdDonation = await _donationRepository.AddAsync(donation);
                _logger.LogInformation(
                    "Donation created: DonationId={DonationId}, CampaignId={CampaignId}, DonorId={DonorId}",
                    message.DonationId,
                    message.CampaignId,
                    message.DonorId);

                success = donation.Status == StatusProcessed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating donation for DonationId: {DonationId}", message.DonationId);
                success = false;
                throw;
            }
            finally
            {
                await _donationProcessedEventPublisher.PublishAsync(message, createdDonation, success);
            }
        }

        private Task<bool> ProcessDonation(
            DonationRequestedEvent message,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (message.DonationAmount <= 0)
            {
                _logger.LogWarning("Donation rejected by local validation: invalid amount. DonationId={DonationId}", message.DonationId);
                return Task.FromResult(false);
            }

            if (message.CampaignId == Guid.Empty || message.DonorId == Guid.Empty)
            {
                _logger.LogWarning("Donation rejected by local validation: invalid campaign or donor. DonationId={DonationId}", message.DonationId);
                return Task.FromResult(false);
            }

            _logger.LogInformation("Donation approved by local validation. DonationId={DonationId}", message.DonationId);
            return Task.FromResult(true);
        }

    }
}
