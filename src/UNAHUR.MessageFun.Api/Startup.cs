
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using Prometheus;
using RabbitMQ.Client;
using Serilog;
using System.Text.Json.Serialization;
using UNAHUR.MessageFun.Api.Consumers;
using UNAHUR.MessageFun.Api.Swagger;
using UNAHUR.MessageFun.Business;
using UNAHUR.MessageFun.Business.Messaging;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]
namespace UNAHUR.MessageFun.Api
{


    public class Startup
    {
        private const string APP_NAME = "UNAHUR.MessageFun";

        static bool? _isRunningInContainer;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public static bool IsRunningInContainer =>
            _isRunningInContainer ??= bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inDocker) && inDocker;


        public void ConfigureServices(IServiceCollection services)
        {


            services.Configure<KestrelServerOptions>(Configuration.GetSection("Kestrel"));
            services.Configure<MessageBusFunConfig>(Configuration.GetSection(MessageBusFunConfig.SECTION_NAME));

            services.AddHealthChecks()
                .AddRabbitMQ((sp) =>
                {
                    var _config = sp.GetRequiredService<IOptions<MessageBusFunConfig>>().Value;
                    if (_config == null || _config.RabbitMQ == null)
                        return null;

                    // RabbitMqHostAddress ayuda a extraer el vhost y port de un unico string localhost:123465/desa
                    var hostAddress = new RabbitMqHostAddress(new Uri(_config.RabbitMQ.Host));

                    return  new ConnectionFactory()
                    {
                        HostName = hostAddress.Host,
                        Port = hostAddress.Port.Value,
                        VirtualHost = hostAddress.VirtualHost,
                        UserName = _config.RabbitMQ.Username,
                        Password = _config.RabbitMQ.Password,
                        // si se desconecta se reconecta solo
                        AutomaticRecoveryEnabled = true
                    };


                    
                    
                }, tags: new[] { "ready" })

                .ForwardToPrometheus();

            services.AddControllers().AddJsonOptions(options =>
            {
                // arregla un problema con los enums en swagger 
                // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1269#issuecomment-586284629
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

            services.AddMassTransit(x =>
            {
                x.AddDelayedMessageScheduler();

                x.AddConsumersFromNamespaceContaining<JobDoneConsumer>();
                x.AddRequestClient<IDoJob>();
                x.AddRequestClient<IGreetingCmd>();
                x.SetKebabCaseEndpointNameFormatter();


                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.UsePrometheusMetrics();

                    var mtConfig = context.GetRequiredService<IOptions<MessageBusFunConfig>>().Value;


                    cfg.Host(mtConfig.RabbitMQ.Host, h =>
                    {
                        h.Username(mtConfig.RabbitMQ.Username);
                        h.Password(mtConfig.RabbitMQ.Password);
                    });
                    cfg.UseDelayedMessageScheduler();
                });
            });
            

            services.AddSwaggerGenExtended(APP_NAME);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            

            app.UseForwardedHeaders();

            app.UseSerilogRequestLogging();


            app.UseMiddleware<ErrorHandlerMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", APP_NAME));

            // global cors policy
            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials

            app.UseRouting();

            app.UseHttpMetrics();
            
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {

                endpoints.MapControllers();
                endpoints.MapMetrics();
                endpoints.MapHealthChecks("/healthz/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                { Predicate = r => false });
                endpoints.MapHealthChecks("/healthz/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    Predicate = registration => registration.Tags.Contains("ready")
                });

            });
        }


    }
}
