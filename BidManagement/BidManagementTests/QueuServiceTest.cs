using Moq;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BidManagement.Services;
using BidManagement.Models;
using Newtonsoft.Json;
using RabbitMQ.Client.Exceptions;

namespace BidManagementTests
{
    [TestFixture]

    public class QueuServiceTest
    {
        private Mock<IConnection> _mockConnection;
        private Mock<IChannel> _mockChannel;
        private IQueueService _mqService;
        private string _queueName = "bidsQueue";

        [SetUp]
        public void SetUp()
        {
            _mockConnection = new Mock<IConnection>();
            _mockChannel = new Mock<IChannel>();

            _mockConnection.Setup(c => c.CreateChannelAsync(null, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(_mockChannel.Object);
  
        }

        [Test]
        public async Task PublishBid_ShouldPublishMessageToQueue()
        {
            var bid = new Bid { Id = 4, CarId = 3, ClientEmail = "client1@example.com", Amount = 450, BidTime = DateTime.Now.AddMinutes(-2) };
            var message = System.Text.Json.JsonSerializer.Serialize(bid);
            var body = Encoding.UTF8.GetBytes(message);
            string correlationId = Guid.NewGuid().ToString();

            var props = new BasicProperties
            {
                CorrelationId = correlationId,
            };
            _mockChannel
                .Setup(c => c.BasicPublishAsync(
                    It.IsAny<string>(), 
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                     It.IsAny<BasicProperties>(),
                    It.IsAny<ReadOnlyMemory<byte>>(),
                    It.IsAny<CancellationToken>()
                ))
                .Returns(ValueTask.CompletedTask);

            _mqService = new QueueService(_mockConnection.Object, _mockChannel.Object, null);
            await _mqService.PublishBid(bid);


            _mockChannel.Verify(c => c.BasicPublishAsync(
                string.Empty,
                 "bidsQueue",
                  false,
                  It.Is<BasicProperties>(props => props.CorrelationId != null), 
                 It.Is<ReadOnlyMemory<byte>>(b => b.ToArray().SequenceEqual(body)),
                  CancellationToken.None
             ), Times.Once);
        }
        [Test]
        public async Task DequeueBidAsync_ShouldReturnBidWhenReceived()
        {
            var bid = new Bid { Id = 4, CarId = 3, ClientEmail = "client1@example.com", Amount = 450, BidTime = DateTime.Now.AddMinutes(-2) };
            var bidJson = System.Text.Json.JsonSerializer.Serialize(bid);
            var body = Encoding.UTF8.GetBytes(bidJson);

            _mockChannel.Setup(c => c.BasicConsumeAsync(
            It.IsAny<string>(), 
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<IDictionary<string, object>>(),
            It.IsAny<IAsyncBasicConsumer>(),
            It.IsAny<CancellationToken>()
        ))
                .Callback<string, bool, string, bool, bool, IDictionary<string, object>, IAsyncBasicConsumer, CancellationToken>(
                    async (queue, autoAck, consumerTag, noLocal, exclusive, arguments, consumer, cancellationToken) =>
                    {
                        var ea = new BasicDeliverEventArgs(
                            consumerTag: "consumerTag",
                            deliveryTag: 1,
                            redelivered: false,
                            exchange: "exchange",
                            routingKey: "routingKey",
                            properties: null,
                            body: body,
                            cancellationToken: CancellationToken.None
                        );

                        if (consumer is AsyncEventingBasicConsumer asyncConsumer)
                        {
                            await asyncConsumer.HandleBasicDeliverAsync(
                                ea.ConsumerTag,
                                ea.DeliveryTag,
                                ea.Redelivered,
                                ea.Exchange,
                                ea.RoutingKey,
                                ea.BasicProperties,
                                ea.Body,
                                ea.CancellationToken
                            );
                        }
                    });

            _mqService = new QueueService(_mockConnection.Object, _mockChannel.Object, null);
            var result = await _mqService.DequeueBidAsync(CancellationToken.None);

            Assert.AreEqual(bid.Id, result.Id);
            Assert.AreEqual(bid.Amount, result.Amount);
            Assert.AreEqual(bid.ClientEmail, result.ClientEmail);
            Assert.AreEqual(bid.CarId, result.CarId);
            _mockChannel.Verify(c => c.BasicAckAsync(1, false, default), Times.Once);

        }


        [Test]
        public void DequeueBidAsync_ShouldHandleCancellation()
        {
            var cts = new CancellationTokenSource();

            cts.Cancel();
            
            _mqService = new QueueService(_mockConnection.Object, _mockChannel.Object, null);
            Assert.ThrowsAsync<TaskCanceledException>(() => _mqService.DequeueBidAsync(cts.Token));
        }

    }
}