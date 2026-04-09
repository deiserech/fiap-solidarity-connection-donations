namespace SolidarityConnection.Donations.Infrastructure.Messaging
{
    public interface IServiceBusPublisher
    {
        Task PublishAsync<T>(T @event, string topicName);
    }
}
