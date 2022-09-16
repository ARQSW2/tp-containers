using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using UNAHUR.MessageFun.Business.Messaging;

namespace UNAHUR.MessageFun.Worker.Consumers
{
    public class GreetingCmdConsumer : IConsumer<IGreetingCmd>
    {
        readonly ILogger<GreetingCmdConsumer> _logger;
        public GreetingCmdConsumer(ILogger<GreetingCmdConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<IGreetingCmd> context)
        {
            _logger.LogInformation("Received Text: {Text}", context.Message.Name);

            return context.RespondAsync(new GreetingCmdResponse
            {
                Saludo = $"Hola {context.Message.Name}"
            }); ;
        }
    }
}
