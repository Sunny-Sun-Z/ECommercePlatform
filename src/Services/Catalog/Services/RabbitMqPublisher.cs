using System;
using System.Text;
using System.Text.Json;
using Catalog.Config;
using Catalog.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Catalog.Services
{
    public interface IRabbitMqPublisher : IDisposable
    {
        void PublishProductCreated(Product product);
    }

    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly IModel _channel;
        private readonly IConnection _connection;
        private readonly RabbitMqSettings _settings;

        public RabbitMqPublisher(IOptions<RabbitMqSettings> options)
        {
            _settings = options.Value;

            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare a durable direct exchange
            _channel.ExchangeDeclare(
                exchange: _settings.ExchangeName,
                type: ExchangeType.Direct,
                durable: true
            );
        }

        public void PublishProductCreated(Product product)
        {
            var payload = JsonSerializer.Serialize(new
            {
                product.Id,
                product.Name,
                product.Price
            });
            var body = Encoding.UTF8.GetBytes(payload);

            _channel.BasicPublish(
                exchange: _settings.ExchangeName,
                routingKey: _settings.RoutingKey,
                basicProperties: null,
                body: body
            );
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
