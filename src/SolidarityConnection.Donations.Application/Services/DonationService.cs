using SolidarityConnection.Donations.Application.DTOs;
using SolidarityConnection.Donations.Application.Interfaces.Publishers;
using SolidarityConnection.Donations.Application.Interfaces.Repositories;
using SolidarityConnection.Donations.Application.Interfaces.Services;
using SolidarityConnection.Donations.Domain.Entities;
using SolidarityConnection.Donations.Domain.Events;
using SolidarityConnection.Donations.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace SolidarityConnection.Donations.Application.Services
{
    public class DonationService : IDonationService
    {
        private readonly IDonationRepository _donationRepository;
        private readonly ICampaignReferenceRepository _campaignRepository;
        private readonly IDonorReferenceRepository _donorRepository;
        private readonly IDonationRequestedEventPublisher _donationRequestedEventPublisher;
        private readonly IDonationProcessedEventPublisher _donationProcessedEventPublisher;
        private readonly ILogger<DonationService> _logger;
        private readonly IConfiguration _configuration;

        public DonationService(
            IDonationRepository donationRepository,
            ICampaignReferenceRepository campaignRepository,
            IDonorReferenceRepository donorRepository,
            IDonationRequestedEventPublisher donationRequestedEventPublisher,
            IDonationProcessedEventPublisher donationProcessedEventPublisher,
            ILogger<DonationService> logger,
            IConfiguration configuration)
        {
            _donationRepository = donationRepository;
            _campaignRepository = campaignRepository;
            _donorRepository = donorRepository;
            _donationRequestedEventPublisher = donationRequestedEventPublisher;
            _donationProcessedEventPublisher = donationProcessedEventPublisher;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task CreateDonationRequest(DonationRequestDto dto)
        {
            await _donationRequestedEventPublisher.PublishAsync(dto);
        }

        public async Task ProcessAsync(DonationRequestedEvent message, CancellationToken cancellationToken)
        {
            var campaign = await _campaignRepository.GetByCodeAsync(message.CampaignCode)
                ?? throw new InvalidOperationException("Campaign not found");

            var donor = await _donorRepository.GetByCodeAsync(message.DonorCode)
                ?? throw new InvalidOperationException("Donor not found");

            bool success = false;

            var donation = new Donation
            {
                ProcessedAt = message.RequestedAt,
                DonorId = donor.Id,
                CampaignId = campaign.Id,
                Amount = message.Amount,
                RequestedAt = message.RequestedAt,
                CorrelationId = message.CorrelationId,
                Status = 1
            };

            Donation? createdDonation = null;
            try
            {
                var paymentResult = await ProcessPayment(message, campaign, donor, cancellationToken);

                donation.Status = paymentResult ? 2 : 3;
                donation.FailureReason = paymentResult ? null : "Payment gateway declined request.";

                createdDonation = await _donationRepository.AddAsync(donation);
                _logger.LogInformation(
                    "Donation created: CampaignCode={CampaignCode}, DonorCode={DonorCode}, CorrelationId={CorrelationId}",
                    message.CampaignCode,
                    message.DonorCode,
                    message.CorrelationId);

                success = paymentResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating donation for CampaignCode: {CampaignCode}, DonorCode: {DonorCode}",
                    message.CampaignCode, message.DonorCode);
                success = false;
                throw;
            }
            finally
            {
                await _donationProcessedEventPublisher.PublishAsync(message, createdDonation, success);
            }
        }

        public Task<Donation?> GetByCampaignAndDonorCodeAsync(int campaignCode, int donorCode)
        {
            return _donationRepository.GetByCampaignAndDonorCodeAsync(campaignCode, donorCode);
        }

        private async Task<bool> ProcessPayment(
            DonationRequestedEvent message,
            CampaignReference campaign,
            DonorReference donor,
            CancellationToken cancellationToken)
        {
            var isGatewayEnabled = bool.TryParse(_configuration["Features:PaymentGateway:Enabled"], out var enabled) && enabled;
            if (!isGatewayEnabled)
            {
                _logger.LogInformation("Payment gateway disabled. Skipping payment processing.");
                return true;
            }

            var baseUrl = _configuration["PaymentGateway:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                _logger.LogError("Payment gateway BaseUrl configuration is missing.");
                return false;
            }

            var route = _configuration["PaymentGateway:FunctionRoute"] ?? "/api/payments/authorize";

            var url = baseUrl.TrimEnd('/') + (route.StartsWith("/") ? route : "/" + route);

            using var httpClient = new HttpClient();

            var requestPayload = new
            {
                requestId = Guid.NewGuid().ToString(),
                donationId = Guid.NewGuid(),
                campaignCode = message.CampaignCode,
                donorCode = message.DonorCode,
                amount = message.Amount,
                currency = "BRL",
                paymentMethod = "credit_card"
            };

            var requestJson = JsonSerializer.Serialize(requestPayload);
            _logger.LogInformation("Sending payment request to gateway: {Url} with payload: {Payload}", url, requestJson);

            HttpResponseMessage response;
            try
            {
                using var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                response = await httpClient.PostAsync(url, content, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling payment gateway function.");
                return false;
            }

            if (!response.IsSuccessStatusCode)
            {
                string? errorBody = null;
                try
                {
                    errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                }
                catch
                {
                    // ignore errors reading error body
                }

                _logger.LogWarning("Payment gateway function returned non-success status code: {StatusCode}. Body: {Body}", response.StatusCode, errorBody);
                return false;
            }

            PaymentGatewayResponseDto? responseContent;
            try
            {
                responseContent = await response.Content.ReadFromJsonAsync<PaymentGatewayResponseDto>(cancellationToken: cancellationToken);
                _logger.LogDebug("Payment approved by Gateway: {@Response}", responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing payment gateway response.");
                return false;
            }

            if (responseContent is null)
            {
                _logger.LogWarning("Payment gateway function returned empty response body.");
                return false;
            }

            var approved = responseContent.Approved && string.Equals(responseContent.Status, "Approved", StringComparison.OrdinalIgnoreCase);
            if (!approved)
            {
                _logger.LogWarning("Payment not approved by gateway. Status: {Status}", responseContent.Status);
            }

            return approved;
        }

        private sealed class PaymentGatewayResponseDto
        {
            public string? PaymentId { get; set; }
            public string? AuthorizationCode { get; set; }
            public string? Status { get; set; }
            public bool Approved { get; set; }
            public DateTime ProcessedAt { get; set; }
        }

    }
}
