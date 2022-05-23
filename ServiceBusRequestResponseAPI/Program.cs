using Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using ServiceBusRequestResponseAPI.ServiceBus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddScoped<IServiceBusRequestResponse, ServiceBusRequestResponseWithTemporaryQueueClient>();
builder.Services.AddScoped<IServiceBusRequestResponse, ServiceBusRequestResponseWithSessionClient>();
builder.Services.AddSingleton<ManagementClient>(new ManagementClient(builder.Configuration.GetValue<string>("ServiceBusConnectionString")));
builder.Services.AddSingleton<ServiceBusClient>(new ServiceBusClient(builder.Configuration.GetValue<string>("ServiceBusConnectionString")));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
