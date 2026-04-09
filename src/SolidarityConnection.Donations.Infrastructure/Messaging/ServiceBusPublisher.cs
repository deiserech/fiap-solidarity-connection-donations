using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace SolidarityConnection.Donations.Infrastructure.Messaging
{
    public class ServiceBusPublisher : IServiceBusPublisher
    {
        private readonly ServiceBusClient _client;

        public ServiceBusPublisher(ServiceBusClient client)
        {
            _client = client;
        }

        public async Task PublishAsync<T>(T @event, string topicName)
        {
            var messageBody = JsonSerializer.Serialize(@event);
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody))
            {
                ContentType = "application/json"
            };

            var currentActivity = Activity.Current;
            if (currentActivity != null)
            {
                message.ApplicationProperties["traceparent"] = currentActivity.Id;
                if (!string.IsNullOrWhiteSpace(currentActivity.TraceStateString))
                {
                    message.ApplicationProperties["tracestate"] = currentActivity.TraceStateString;
                }
            }

            var sender = _client.CreateSender(topicName);
            await sender.SendMessageAsync(message);
        }
    }
}
