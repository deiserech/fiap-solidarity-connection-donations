using Azure.Messaging.ServiceBus;
using SolidarityConnection.Donations.Domain.Events;
using SolidarityConnection.Donations.Infrastructure.ServiceBus;
using SolidarityConnection.Donations.Shared.Tracing;
using Newtonsoft.Json;

namespace SolidarityConnection.Donations.Api.BackgroundServices.Campaigns
{
    public class CampaignUpsertedConsumer : BackgroundService
    {
        private readonly IServiceBusClientWrapper _sb;
        private readonly IServiceScopeFactory _scopeFactory;
        private IServiceBusProcessor? _processor;
        private readonly IConfiguration _config;
        private readonly ILogger<CampaignUpsertedConsumer> _logger;

        public CampaignUpsertedConsumer(
            IServiceBusClientWrapper sb,
            IServiceScopeFactory scopeFactory,
            IConfiguration config,
            ILogger<CampaignUpsertedConsumer> logger)
        {
            _sb = sb;
            _scopeFactory = scopeFactory;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var topic = _config["CAMPAIGN_TOPIC"] ?? "campaigns-upserted";
            var subscription = _config["CAMPAIGN_SUBSCRIPTION"] ?? "fiap-cloud-games-payments";
            try
            {
                _processor = _sb.CreateProcessorWrapper(topic, subscription);
                _processor.ProcessMessageAsync += async args =>
                {
                    var message = args.Message;

                    using var activity = ServiceBusTracingHelper.StartConsumerActivity(
                        message,
                        "Donations.CampaignUpsertedConsumer.Process",
                        topic,
                        subscription);

                    var body = message.Body.ToString();
                    var msg = JsonConvert.DeserializeObject<CampaignUpsertedEvent>(body);
                    if (msg == null)
                    {
                        await args.CompleteMessageAsync(message);
                        return;
                    }

                    using var scope = _scopeFactory.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<ICampaignUpsertedMessageHandler>();
                    await handler.HandleAsync(msg, args.CancellationToken);

                    await args.CompleteMessageAsync(message);
                };

                _processor.ProcessErrorAsync += ErrorHandler;
                await _processor.StartProcessingAsync(stoppingToken);
            }
            catch (ServiceBusException ex)
            {
                _logger.LogError(ex, "ServiceBus error creating/starting processor for topic {Topic}, subscription {Subscription}", topic, subscription);
                return;
            }
        }

        private Task ErrorHandler(ProcessErrorEventArgs arg)
        {
            _logger.LogError($"CampaignUpsertedConsumer error: {arg.Exception}");
            return Task.CompletedTask;
        }
    }
}
