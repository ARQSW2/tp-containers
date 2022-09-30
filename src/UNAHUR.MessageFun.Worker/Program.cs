namespace UNAHUR.MessageFun.Worker
{
    using MassTransit;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using Prometheus;
    using RabbitMQ.Client;
    using Serilog;
    using System;
    using UNAHUR.MessageFun.Business;
    using UNAHUR.MessageFun.Business.Messaging;
    using UNAHUR.MessageFun.Worker.Consumers;
    using UNAHUR.MessageFun.Worker.Serrvices;
    using UNAHUR.MessageFun.Worker.Services;

    public class Program
    {
        static bool? _isRunningInContainer;
        public static bool IsRunningInContainer =>
            _isRunningInContainer ??= bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inDocker) && inDocker;

        public static void Main(string[] args)
        {
            try
            {
                Serilog.Debugging.SelfLog.Enable(Console.Error);
                Log.Information("Building host..");
                var host = CreateHostBuilder(args).Build();

                Log.Information("Building host OK");
                Log.Information("Running host host...");
                host.Run();

            }
            catch (Exception ex)
            {
                Console.Error.Write($"Faltal error {ex.Message}");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
             .UseSerilog((ctx, logConfig) =>
             {
                 logConfig.ReadFrom.Configuration(ctx.Configuration);
             })
            .ConfigureServices((hostContext, services) =>
            {
                // configura la seccion que contiene los datos de conexion de masstransi
                services.Configure<MessageBusFunConfig>(hostContext.Configuration.GetSection(MessageBusFunConfig.SECTION_NAME));

                var sp = services.BuildServiceProvider();

                var config = sp.GetRequiredService<IOptions<MessageBusFunConfig>>().Value;

                services.AddMassTransit(x =>
                {
                    x.AddDelayedMessageScheduler();

                    x.AddConsumersFromNamespaceContaining<DoJobConsumer>();

                    x.AddRequestClient<IJobDone>();

                    if (!string.IsNullOrWhiteSpace(config.SagasRedis))
                    {                     // maquina de estados qye rastrea el estado de lso jobs
                        x.AddSagaRepository<JobSaga>()
                            .RedisRepository(r =>
                            {
                                r.DatabaseConfiguration(config.SagasRedis);

                                // Default is Optimistic
                                r.ConcurrencyMode = ConcurrencyMode.Pessimistic;

                                // Optional, prefix each saga instance key with the string specified
                                // resulting dev:c6cfd285-80b2-4c12-bcd3-56a00d994736
                                r.KeyPrefix = "dev";

                                // Optional, to customize the lock key
                                r.LockSuffix = "-lockage";

                                // Optional, the default is 30 seconds
                                r.LockTimeout = TimeSpan.FromSeconds(90);
                            });

                        // maquina de estados qye rastrea el estado de lso jobs
                        x.AddSagaRepository<JobTypeSaga>()
                            .RedisRepository(r =>
                            {
                                r.DatabaseConfiguration(config.SagasRedis);

                                // Default is Optimistic
                                r.ConcurrencyMode = ConcurrencyMode.Pessimistic;

                                // Optional, prefix each saga instance key with the string specified
                                // resulting dev:c6cfd285-80b2-4c12-bcd3-56a00d994736
                                r.KeyPrefix = "dev";

                                // Optional, to customize the lock key
                                r.LockSuffix = "-lockage";

                                // Optional, the default is 30 seconds
                                r.LockTimeout = TimeSpan.FromSeconds(90);
                            });

                        //rastrea los intentos de cada job
                        x.AddSagaRepository<JobAttemptSaga>().RedisRepository(r =>
                        {
                            r.DatabaseConfiguration(config.SagasRedis);

                            // Default is Optimistic
                            r.ConcurrencyMode = ConcurrencyMode.Pessimistic;

                            // Optional, prefix each saga instance key with the string specified
                            // resulting dev:c6cfd285-80b2-4c12-bcd3-56a00d994736
                            r.KeyPrefix = "dev";

                            // Optional, to customize the lock key
                            r.LockSuffix = "-lockage";

                            // Optional, the default is 30 seconds
                            r.LockTimeout = TimeSpan.FromSeconds(90);
                        }); ;

                    }

                    x.SetKebabCaseEndpointNameFormatter();

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.UsePrometheusMetrics();

                        cfg.Host(config.RabbitMQ.Host, h =>
                        {
                            h.Username(config.RabbitMQ.Username);
                            h.Password(config.RabbitMQ.Password);
                        });

                        cfg.UseDelayedMessageScheduler();

                        var options = new ServiceInstanceOptions()
                            .EnableJobServiceEndpoints()
                            .SetEndpointNameFormatter(context.GetService<IEndpointNameFormatter>() ?? KebabCaseEndpointNameFormatter.Instance);

                        cfg.ServiceInstance(options, instance =>
                        {
                            instance.ConfigureJobServiceEndpoints(js =>
                            {
                                js.FinalizeCompleted = true;
                                if(!string.IsNullOrWhiteSpace(config.SagasRedis))
                                    js.ConfigureSagaRepositories(context);
                            });
                            instance.ConfigureEndpoints(context);
                        });
                    });
                });
                services.AddHealthChecks()
                    .AddRabbitMQ((sp) =>
                    {
                        var _config = sp.GetRequiredService<IOptions<MessageBusFunConfig>>().Value;
                        if (_config == null || _config.RabbitMQ == null)
                            return null;

                        // RabbitMqHostAddress ayuda a extraer el vhost y port de un unico string localhost:123465/desa
                        var hostAddress = new RabbitMqHostAddress(new Uri(_config.RabbitMQ.Host));

                        return new ConnectionFactory()
                        {
                            HostName = hostAddress.Host,
                            Port = hostAddress.Port.Value,
                            VirtualHost = hostAddress.VirtualHost,
                            UserName = _config.RabbitMQ.Username,
                            Password = _config.RabbitMQ.Password,
                            // si se desconecta se reconecta solo
                            AutomaticRecoveryEnabled = true
                        };
                    }, tags: new[] { "ready" }); 
                services.AddHostedService<HealthcheckHttpListener>();
                services.AddHostedService<MetricsServerHost>();

            });
    }
}
