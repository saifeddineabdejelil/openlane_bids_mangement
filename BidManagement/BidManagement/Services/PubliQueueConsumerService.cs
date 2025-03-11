using BidManagement.Models;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client.Exceptions;
using System.Security.Cryptography;
namespace BidManagement.Services
{
    public class PubliQueueConsumerService : BackgroundService
    {
        private  IConnection _connection;
        private IChannel _channel;
        private readonly string _hostname = "localhost"; 
        private readonly string _queueName = "publicQueue";
        private readonly ILogger<QueueService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly IBidService _bidService;
        private readonly IQueueService _queueService;
        public PubliQueueConsumerService(IConnection connection, IChannel channel,
            ILogger<QueueService> logger, IBidService bidService, IQueueService queueService)
        {
            _connection = connection;
            _channel = channel;
            _logger = logger;
            _bidService = bidService;
            _queueService = queueService;
            _retryPolicy = Policy
           .Handle<BrokerUnreachableException>()
           .Or<OperationInterruptedException>()
           .WaitAndRetryAsync(
               retryCount: 3,
               sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
               onRetry: (exception, retryCount, context) =>
               {
                   Console.WriteLine($"Retry Failed");
               });
            
        }
        public async Task InitializeAsync()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            await Task.CompletedTask;
        }

        // listner for public in queue (azzure rabbitmq for example ) to consume bids pushed by clients
        // to be updated
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                var bid = await DequeueBidAsync(stoppingToken);
                await _bidService.SaveBidAsync(bid);

                _logger.LogInformation($"Bid  ( {bid.Id} ) saved successfully.");

                await _queueService.PublishBid(bid);

                _logger.LogInformation($"Bid  ( {bid.Id} ) pushed in queue successfully.");

            }

        }
        public async Task<Bid> DequeueBidAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            var tcs = new TaskCompletionSource<Bid>();

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    if (tcs.Task.IsCompleted)
                    {
                Console.WriteLine("TaskCompletionSource already completed. Skipping message processing.");
                        return;
                    }
                    var body = ea.Body.ToArray();
                    var bidJson = Encoding.UTF8.GetString(body);
                    var bid = JsonConvert.DeserializeObject<Bid>(bidJson);

                    tcs.SetResult(bid);

                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    Console.WriteLine($"Succes Deqeue bid: {bid.ClientEmail}");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing bid: {ex.Message}");
                    _logger.LogError(ex, "error when processing bid message");

                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);

                    tcs.SetException(ex);
                }
            };

            await _channel.BasicConsumeAsync("bidsQueue", autoAck: false, consumer);

            using (stoppingToken.Register(() => tcs.TrySetCanceled()))
            {
                return await tcs.Task;
            }
        }

    }
}