﻿using BidManagement.Models;
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
    public class QueueService : IQueueService
    {
        private  IConnection _connection;
        private IChannel _channel;
        private readonly string _hostname = "localhost"; 
        private readonly string _queueName = "bidsQueue";
        private readonly ILogger<QueueService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        public QueueService(IConnection connection, IChannel channel, ILogger<QueueService> logger)
        {
            _connection = connection;
            _channel = channel;
            _logger = logger;
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
        public async Task PublishBid(Bid bid)
        {
            var message = System.Text.Json.JsonSerializer.Serialize(bid);
            var body = Encoding.UTF8.GetBytes(message);
            string correlationId = Guid.NewGuid().ToString();

            var props = new BasicProperties
            {
                CorrelationId = correlationId,
            };

            try
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await _channel.BasicPublishAsync(
                        exchange: string.Empty,
                        routingKey: _queueName,
                        mandatory: false,
                        basicProperties: props,
                        body: new ReadOnlyMemory<byte>(body),
                        cancellationToken: CancellationToken.None
                    );

                    Console.WriteLine($"pushed {bid.ClientEmail} ");
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to publish bid after retries: {ex.Message}");
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