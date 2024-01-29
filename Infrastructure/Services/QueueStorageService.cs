using Azure.Storage.Queues;
using Domain.Interfaces;

namespace Infrastructure.Services
{
    public class QueueStorageService : IQueueStorageService
    {
        private static readonly QueueClientOptions _options = new QueueClientOptions
        {
            MessageEncoding = QueueMessageEncoding.Base64
        };

        public Task UploadNewMessageToQueueAsync(string connectionString, string queueName, string message)
        {
            ArgumentException.ThrowIfNullOrEmpty(connectionString);
            ArgumentException.ThrowIfNullOrEmpty(queueName);

            return UploadNewMessageToQueueInternalAsync(connectionString, queueName, message);
        }

        private static async Task UploadNewMessageToQueueInternalAsync(string connectionString, string queueName, string message)
        {
            var cloudQueue = GetQueueClient(connectionString, queueName);
            await cloudQueue.SendMessageAsync(message);
        }

        private static QueueClient GetQueueClient(string connectionString, string queueName)
        {
            var queueServiceClient = new QueueServiceClient(connectionString, _options);
            var queueClient = queueServiceClient.GetQueueClient(queueName);
            queueClient.CreateIfNotExists();
            return queueClient;
        }
    }
}
