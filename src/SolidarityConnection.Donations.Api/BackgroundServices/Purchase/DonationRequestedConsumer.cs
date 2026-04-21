using Azure.Messaging.ServiceBus;
using SolidarityConnection.Donations.Domain.Events;
using SolidarityConnection.Donations.Infrastructure.ServiceBus;
using SolidarityConnection.Donations.Shared.Tracing;
using Newtonsoft.Json;

namespace SolidarityConnection.Donations.Api.BackgroundServices.Donations
{
    public class DonationRequestedConsumer : BackgroundService
    {
        private readonly IServiceBusClientWrapper _sb;
        private IServiceBusProcessor? _processor;
        private readonly IConfiguration _config;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DonationRequestedConsumer> _logger;

        public DonationRequestedConsumer(
            IServiceBusClientWrapper sb,
            IConfiguration config,
            IServiceScopeFactory scopeFactory,
            ILogger<DonationRequestedConsumer> logger)
        {
            _sb = sb;
            _config = config;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var topic = _config["DONATION_TOPIC"] ?? "donation-requested";
            var subscription = _config["DONATION_SUBSCRIPTION"] ?? "solidarity-connection-donations-api";
            try
            {
                _processor = _sb.CreateProcessorWrapper(topic, subscription);
                _processor.ProcessMessageAsync += async args =>
                {
                    var message = args.Message;

                    using var activity = ServiceBusTracingHelper.StartConsumerActivity(
                        message,
                        "Donations.DonationRequestedConsumer.Process",
                        topic,
                        subscription);

                    var body = message.Body.ToString();
                    var msg = JsonConvert.DeserializeObject<DonationRequestedEvent>(body);
                    if (msg == null)
                    {
                        await args.CompleteMessageAsync(message);
                        return;
                    }

                    using var scope = _scopeFactory.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<IDonationRequestedMessageHandler>();
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
            _logger.LogError($"DonationRequestedConsumer error: {arg.Exception}");
            return Task.CompletedTask;
        }
    }
}
