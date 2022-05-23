using Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System.Text.Json;

namespace ServiceBusRequestResponseAPI.ServiceBus
{
    public class ServiceBusRequestResponseWithTemporaryQueueClient
        : IServiceBusRequestResponse
    {
        private readonly ILogger<ServiceBusRequestResponseWithTemporaryQueueClient> logger;
        private readonly ServiceBusClient serviceBusClient;
        private readonly ManagementClient managementClient;

        public ServiceBusRequestResponseWithTemporaryQueueClient(
            ILogger<ServiceBusRequestResponseWithTemporaryQueueClient> logger,
            ServiceBusClient serviceBusClient,
            ManagementClient managementClient)
        {
            this.logger = logger;
            this.serviceBusClient = serviceBusClient;
            this.managementClient = managementClient;
        }
        public async Task<TResponse> SendAsync<TResponse>(string queue, object @object)            
            where TResponse : class
        {
            var isTemporaryQueueCreated = false;
            var tempQueue = System.Guid.NewGuid().ToString();

            try
            {
                var sender = this.serviceBusClient.CreateSender(queue);                
                
                var tempQueueCreationDetail = new QueueDescription(tempQueue)
                {
                    AutoDeleteOnIdle = TimeSpan.FromMinutes(5),
                    MaxDeliveryCount = 1
                };
                var responseCreateQueue = await this.managementClient.CreateQueueAsync(tempQueueCreationDetail);

                if (responseCreateQueue != null)
                {
                    isTemporaryQueueCreated = true;
                }

                var receiver = this.serviceBusClient.CreateReceiver(tempQueue);

                var requestMessage = new ServiceBusMessage(new BinaryData(JsonSerializer.SerializeToUtf8Bytes(@object)))
                {
                    ReplyTo = tempQueue,
                };

                // This will help processor to identify request type.
                requestMessage.ApplicationProperties.Add("RequestType", @object.GetType().FullName);
                await sender.SendMessageAsync(requestMessage);

                // Wait for response.
                var receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));

                var response = JsonSerializer.Deserialize<TResponse>(receivedMessage.Body.ToStream());

                return response!;
            }
            finally
            {
                if (isTemporaryQueueCreated)
                {
                    await this.managementClient.DeleteQueueAsync(tempQueue);
                }                
            }

            return default;
        }
    }
}
