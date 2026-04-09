using Azure.Messaging.ServiceBus;
using SolidarityConnection.Donations.Domain.Events;
using SolidarityConnection.Donations.Infrastructure.ServiceBus;
using SolidarityConnection.Donations.Shared.Tracing;
using Newtonsoft.Json;

namespace SolidarityConnection.Donations.Api.BackgroundServices.Donors
{
    public class DonorUpsertedConsumer : BackgroundService
    {
        private readonly IServiceBusClientWrapper _sb;
        private IServiceBusProcessor? _processor;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<DonorUpsertedConsumer> _logger;

        public DonorUpsertedConsumer(
            IServiceBusClientWrapper sb,
            IServiceScopeFactory scopeFactory,
            IConfiguration config,
            ILogger<DonorUpsertedConsumer> logger)
        {
            _sb = sb;
            _scopeFactory = scopeFactory;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var topic = _config["DONOR_TOPIC"] ?? "donors-upserted";
            var subscription = _config["DONOR_SUBSCRIPTION"] ?? "solidarity-connection-donations-api";
            try
            {
                _processor = _sb.CreateProcessorWrapper(topic, subscription);
                _processor.ProcessMessageAsync += async args =>
                {
                    var message = args.Message;

                    using var activity = ServiceBusTracingHelper.StartConsumerActivity(
                        message,
                        "Donations.DonorUpsertedConsumer.Process",
                        topic,
                        subscription);

                    var body = message.Body.ToString();
                    var msg = JsonConvert.DeserializeObject<DonorUpsertedEvent>(body);
                    if (msg == null)
                    {
                        await args.CompleteMessageAsync(message);
                        return;
                    }

                    using var scope = _scopeFactory.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<IDonorUpsertedMessageHandler>();
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
            _logger.LogError($"DonorUpsertedConsumer error: {arg.Exception}");
            return Task.CompletedTask;
        }
    }
}
