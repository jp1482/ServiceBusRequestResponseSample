using Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System.Text.Json;

namespace ServiceBusRequestResponseAPI.ServiceBus
{
    public class ServiceBusRequestResponseWithSessionClient
        : IServiceBusRequestResponse
    {
        private readonly ILogger<ServiceBusRequestResponseWithSessionClient> logger;
        private readonly ServiceBusClient serviceBusClient;
        private readonly string queueName;

        public ServiceBusRequestResponseWithSessionClient(
            ILogger<ServiceBusRequestResponseWithSessionClient> logger,
            ServiceBusClient serviceBusClient)
        {
            this.logger = logger;
            this.serviceBusClient = serviceBusClient;            
        }
        public async Task<TResponse> SendAsync<TResponse>(string queue, object @object)            
            where TResponse : class
        {            
            var replyToSessionId = System.Guid.NewGuid().ToString();
            var replyToQueue = $"{queue}-response";
            try
            {
                var sender = this.serviceBusClient.CreateSender(queue);  

                var receiver = await this.serviceBusClient.AcceptSessionAsync(replyToQueue,replyToSessionId);

                var requestMessage = new ServiceBusMessage(new BinaryData(JsonSerializer.SerializeToUtf8Bytes(@object)))
                {
                    ReplyTo = replyToQueue,
                    ReplyToSessionId = replyToSessionId,
                };

                // This will help processor to identify request type.
                requestMessage.ApplicationProperties.Add("RequestType", @object.GetType().FullName);
                await sender.SendMessageAsync(requestMessage);

                // Wait for response.
                var receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));                
                var response = JsonSerializer.Deserialize<TResponse>(receivedMessage.Body.ToStream());
                await receiver.CompleteMessageAsync(receivedMessage);
                return response!;
            }
            finally
            {
                    
            }

            return default;
        }
    }
}
