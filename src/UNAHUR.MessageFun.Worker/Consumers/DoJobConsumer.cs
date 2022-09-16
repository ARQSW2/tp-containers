using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UNAHUR.MessageFun.Business.Messaging;

namespace UNAHUR.MessageFun.Worker.Consumers
{
    /// <summary>
    /// Consumidor del job <see cref="IDoJob"/>
    /// </summary>
    public class DoJobConsumer :
        IJobConsumer<IDoJob>
    {
        readonly ILogger _logger;

        public DoJobConsumer(ILogger<DoJobConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Run(JobContext<IDoJob> context)
        {

            try
            {
                _logger.LogInformation($"Procesing job: {context.Job.Path}:{context.Job.GroupId}...");
                var rng = new Random();

                //await Task.Delay(5000);
                // esto anda medio mal
                await context.Publish<IJobDone>(context.Job);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al procesar job {context.Job.Path}:{context.Job.GroupId}...", ex);
            }


        }
    }
    /// <summary>
    /// 
    /// <see cref="DoJobConsumer"/> Configuration
    /// </summary>
    public class DoJobConsumerDefinition :
       ConsumerDefinition<DoJobConsumer>
    {
        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<DoJobConsumer> consumerConfigurator)
        {
            consumerConfigurator.Options<JobOptions<IDoJob>>(options =>
                options.SetRetry(r => r.Interval(3, TimeSpan.FromSeconds(30))).SetJobTimeout(TimeSpan.FromMinutes(10)).SetConcurrentJobLimit(1));
        }
    }


}