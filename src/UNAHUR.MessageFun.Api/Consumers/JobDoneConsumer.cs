using MassTransit;
using UNAHUR.MessageFun.Business.Messaging;

namespace UNAHUR.MessageFun.Api.Consumers
{
    /// <summary>
    /// <see cref="IConsumer"/> for <see cref="IJobDone"/> message
    /// </summary>
    public class JobDoneConsumer : IConsumer<IJobDone>
    {
        readonly ILogger _logger;

        public JobDoneConsumer(ILogger<IJobDone> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<IJobDone> context)
        {

            _logger.LogInformation($"Completed job: {context.Message.GroupId}");

            return Task.CompletedTask;
        }
    }
}
