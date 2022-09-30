using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UNAHUR.MessageFun.Business;
using static MassTransit.ValidationResultExtensions;

namespace UNAHUR.MessageFun.Worker.Services
{
    /// <summary>
    /// Background service hosting Healthcheck's HttpListener
    /// </summary>
    public class HealthcheckHttpListener : BackgroundService
    {
        private readonly ILogger<HealthcheckHttpListener> _logger;
        private readonly HealthCheckService _healthCheckService;
        private readonly HttpListener _httpListener;
        private readonly MessageBusFunConfig _configuration;


        public HealthcheckHttpListener(HealthCheckService healthCheckService, ILogger<HealthcheckHttpListener> logger, IOptions<MessageBusFunConfig> configuration)
        {
            _logger = logger;
            _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
            _configuration = configuration.Value ?? throw new ArgumentNullException(nameof(configuration));

            _httpListener = new HttpListener();
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            var healthBase = _configuration.Metrics.HealthPrefix;

            if (!healthBase.EndsWith("/"))
                healthBase += "/";

            _logger.LogInformation($"Healthcheck init on {healthBase}");

            _httpListener.Prefixes.Add(healthBase + "live/");
            _httpListener.Prefixes.Add(healthBase + "ready/");

            _httpListener.Start();
            _logger.LogInformation($"Healthcheck listening {healthBase}");

            while (!stoppingToken.IsCancellationRequested)
            {
                HttpListenerContext ctx = null;
                try
                {
                    ctx = await _httpListener.GetContextAsync();
                }
                catch (HttpListenerException ex)
                {
                    if (ex.ErrorCode == 995) return;
                }


                if (ctx == null) continue;


                bool isHealthy = true;
                if (ctx.Request.RawUrl.Contains("/healthz/live/"))
                {
                    var health = await _healthCheckService.CheckHealthAsync(registration => !registration.Tags.Contains("ready"), stoppingToken);
                    isHealthy = health.Status == HealthStatus.Healthy;
                }
                else
                {
                    var health = await _healthCheckService.CheckHealthAsync(registration => registration.Tags.Contains("ready"), stoppingToken);
                    isHealthy = health.Status == HealthStatus.Healthy;
                }
                var pstrResponse = isHealthy ? "Healthy" : "Unhealthy";

                var response = ctx.Response;
                response.ContentType = "text/plain";
                response.Headers.Add(HttpResponseHeader.CacheControl, "no-store, no-cache");
                response.StatusCode = (int)HttpStatusCode.OK;

                var messageBytes = Encoding.UTF8.GetBytes(pstrResponse);
                response.ContentLength64 = messageBytes.Length;
                await response.OutputStream.WriteAsync(messageBytes, 0, messageBytes.Length);
                response.OutputStream.Close();
                response.Close();
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _httpListener.Stop();
            await base.StopAsync(cancellationToken);
        }
    }
}
