// See https://aka.ms/new-console-template for more information
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceBusRequestResponse.Models;

var builder = Host.CreateDefaultBuilder();
builder.ConfigureServices((context,services) =>
{
    services.AddHostedService<SimpleRequestProcessor>();
    services.AddSingleton<ServiceBusClient>(new ServiceBusClient(context.Configuration.GetValue<string>("ServiceBusConnectionString")));
});
var host = builder.Build();
await host.RunAsync();

public class SimpleRequestProcessor : BackgroundService
{
    private readonly ILogger<SimpleRequestProcessor> logger;
    private readonly ServiceBusClient serviceBusClient;

    public SimpleRequestProcessor(ILogger<SimpleRequestProcessor> logger,
        ServiceBusClient serviceBusClient)
    {
        this.logger = logger;
        this.serviceBusClient = serviceBusClient;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var processor = this.serviceBusClient.CreateProcessor("myrequest");
        processor.ProcessMessageAsync += Processor_ProcessMessageAsync;
        processor.ProcessErrorAsync += Processor_ProcessErrorAsync;
        this.logger.LogInformation("From Service");
        await processor.StartProcessingAsync(stoppingToken);  
    }

    private Task Processor_ProcessErrorAsync(ProcessErrorEventArgs arg)
    {
        return Task.CompletedTask;
    }

    private async Task Processor_ProcessMessageAsync(ProcessMessageEventArgs arg)
    {
        Console.WriteLine("Message Received");
        string replyToQueue = arg.Message.ReplyTo;
        string sessionId = arg.Message.ReplyToSessionId;
        try
        {
            var sender = this.serviceBusClient.CreateSender(replyToQueue);
            var list = new List<TodoItem>()
            {
                new(){ Id = 1, Title = "First todo"}
            };
            var message = new ServiceBusMessage(new BinaryData(System.Text.Json.JsonSerializer.Serialize(list)));
            if (!string.IsNullOrEmpty(sessionId))
            {
                message.SessionId = sessionId;
            }
            await sender.SendMessageAsync(message);
        }
        catch
        {

        }
        await arg.CompleteMessageAsync(arg.Message);
    }
}
