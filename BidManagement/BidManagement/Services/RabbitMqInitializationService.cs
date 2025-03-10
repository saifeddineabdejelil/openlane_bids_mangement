using RabbitMQ.Client;

namespace BidManagement.Services
{
    public class RabbitMqInitializationService : IHostedService
    {
        private  IConnection _connection;
        private  IChannel _channel;

        public RabbitMqInitializationService(IConnection connection, IChannel channel)
        {
            _connection = connection;
            _channel = channel;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = await factory.CreateConnectionAsync();

            _channel = await _connection.CreateChannelAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();
            }
            if (_connection != null)
            {
                await _connection.CloseAsync();
            }
        }
    }
}
