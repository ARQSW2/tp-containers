using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UNAHUR.MessageFun.Business;

namespace UNAHUR.MessageFun.Worker.Serrvices
{
    public class MetricsServerHost : BackgroundService
    {
        private readonly ILogger<MetricsServerHost> _logger;
        private MetricServer _metricServer;

        private readonly MessageBusFunConfig config;

        public MetricsServerHost(ILogger<MetricsServerHost> logger, IOptions<MessageBusFunConfig> configuration)
        {
            _logger = logger;

            config = configuration.Value ?? throw new ArgumentNullException(nameof(configuration));
            

            

        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Starting Metrics server on  {config.Metrics.Host}:{config.Metrics.Port}/metrics");
            try
            {
                _metricServer = new MetricServer(config.Metrics.Host, config.Metrics.Port);
                _metricServer.Start();
                _logger.LogInformation($"Metrics server started");
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Could not start metrics server");
            }
            
            
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_metricServer != null)
            {
                await _metricServer.StopAsync();
                _metricServer = null;
            }
            await base.StopAsync(cancellationToken);
        }
    }
}
