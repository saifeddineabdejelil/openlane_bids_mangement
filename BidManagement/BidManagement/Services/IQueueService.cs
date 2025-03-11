using BidManagement.Models;

namespace BidManagement.Services
{
    public interface IQueueService
    {
        Task PublishBid(Bid bid);
        Task InitializeAsync();
        Task<Bid> DequeueBidAsync(CancellationToken stoppingToken);
        //Task<uint> GetQueueLengthAsync();
    }
}
