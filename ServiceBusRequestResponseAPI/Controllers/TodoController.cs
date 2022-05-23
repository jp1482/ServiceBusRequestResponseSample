using Microsoft.AspNetCore.Mvc;
using ServiceBusRequestResponse.Models;
using ServiceBusRequestResponseAPI.ServiceBus;

namespace ServiceBusRequestResponseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {      

        private readonly ILogger<TodoController> _logger;
        private readonly IServiceBusRequestResponse serviceBusRequestResponse;

        public TodoController(ILogger<TodoController> logger,
            IServiceBusRequestResponse serviceBusRequestResponse)
        {
            _logger = logger;
            this.serviceBusRequestResponse = serviceBusRequestResponse;
        }

        [HttpGet]
        public async Task<IEnumerable<TodoItem>> Get()
        {
            var response = await this.serviceBusRequestResponse.SendAsync<List<TodoItem>>("myrequest", new GetAllTodoItems()
            {
                
            });
            return response;
        }

        
    }
}