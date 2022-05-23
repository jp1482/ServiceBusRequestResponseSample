namespace ServiceBusRequestResponseAPI.ServiceBus
{
    public interface IServiceBusRequestResponse
    {
        Task<TResponse> SendAsync<TResponse>(string queue, object @object)
            where TResponse : class;
            
    }
}
