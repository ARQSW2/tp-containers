namespace UNAHUR.MessageFun.Business
{
    /// <summary>
    /// COnfiguration class using IOptionsPattern
    /// </summary>
    public class MessageBusFunConfig
    {
        /// <summary>
        /// Section name on the appsettings.json
        /// </summary>
        public const string SECTION_NAME = "MessageBusFun";

        /// <summary>
        /// RabbitMQ connection string
        /// </summary>
        public RabbitMQConfig RabbitMQ { get; set; }

        /// <summary>
        /// Parametros para la exposicion de metricas de prometheus
        /// </summary>
        public MetricsConfig Metrics { get; set; }

        /// <summary>
        /// Redis for SAGAS Pattern
        /// </summary>
        public string SagasRedis { get; set; }

        public MessageBusFunConfig()
        {
            RabbitMQ = new RabbitMQConfig();
            Metrics = new MetricsConfig();

        }
    }
    /// <summary>
    /// RabbitMQ Configuration Sections
    /// </summary>
    public class RabbitMQConfig
    {
        /// <summary>
        /// The URI host address of the RabbitMQ host (rabbitmq://host:port/vhost)
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// Rabbitn MQ User name
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// RabbitMQ Password
        /// </summary>
        public string Password { get; set; }
    }

    public class MetricsConfig
    {
        public string HealthPrefix { get; set; }
        public int Port { get; set; }
        public MetricsConfig()
        {
            HealthPrefix = "http://localhost:9091/healthz/";
            this.Port = 0;
        }

    }

}