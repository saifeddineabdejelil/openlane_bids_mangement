using BidManagement.Models;

namespace BidManagement.Services
{
    public interface IQueueService
    {
        void PublishBid(Bid bid);
        Task InitializeAsync();
        Task<Bid> DequeueBidAsync(CancellationToken stoppingToken);
    }
}
