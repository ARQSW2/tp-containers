using RabbitMQ.Client;
using System.Text.Json;


namespace RabbitMQ.Client
{
    public static class RabbitMQModelExtensions
    {
        /// <summary>
        /// Metodo de extension para enviar mensajes a rabbit
        /// </summary>
        /// <param name="channel">Item a extender</param>
        /// <param name="data">Informaicon a enviar</param>
        /// <param name="exchangeName">Nombre del excvhange</param>
        /// <param name="exchangeType">Tipo de exchange</param>
        /// <param name="routing">Clave de ruteo</param>
        public static void SendMessage(this IModel channel, object data, string exchangeName, string exchangeType, string routing)
        {
            string message = JsonSerializer.Serialize(data);
            var body = System.Text.Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(
                        exchange: $"{exchangeName.ToUpper()}_{exchangeType.ToUpper()}",
                        routingKey: routing,
                        basicProperties: null,
                        body: body);

        }

    }


}
