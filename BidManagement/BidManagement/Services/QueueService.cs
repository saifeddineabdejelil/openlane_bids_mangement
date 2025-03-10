using BidManagement.Models;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using System.Threading;
namespace BidManagement.Services
{
    public class QueueService : IQueueService
    {
        private  IConnection _connection;
        private IChannel _channel;
        private readonly string _hostname = "localhost"; 
        private readonly string _queueName = "bidsQueue";

        public QueueService(IConnection connection, IChannel channel)
        {
            _connection = connection;
            _channel = channel;
        }
        public async Task InitializeAsync()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            await Task.CompletedTask;
        }
        public async Task PublishBid(Bid bid)
        {
            var message = System.Text.Json.JsonSerializer.Serialize(bid);
            var body = Encoding.UTF8.GetBytes(message);
            string correlationId = Guid.NewGuid().ToString();

            var props = new BasicProperties
            {
                CorrelationId = correlationId,
            };

            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: "",
                mandatory: false,
                basicProperties: props,
                body: new ReadOnlyMemory<byte>(body),
                cancellationToken: CancellationToken.None
            );

            Console.WriteLine($" [x] Sent {message}");

        }

        public async Task<Bid> DequeueBidAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            var tcs = new TaskCompletionSource<Bid>();

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var bidJson = Encoding.UTF8.GetString(body);
                var bid = JsonConvert.DeserializeObject<Bid>(bidJson);

                tcs.SetResult(bid);

                _channel.BasicAckAsync(ea.DeliveryTag, false);
            };

            await _channel.BasicConsumeAsync("bidsQueue", false, consumer);
            using (stoppingToken.Register(() => tcs.TrySetCanceled()))
            {
                return await tcs.Task;
            }

        }

    }
}