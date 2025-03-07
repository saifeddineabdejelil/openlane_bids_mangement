using BidManagement.Models;

namespace BidManagement.Repositories
{
    public interface IBidRepository
    {
        Task SaveBidAsync(Bid bid);
        Task<Bid> GetByBidId(int bidId);
        Task<IEnumerable<Bid>> GetBidsByCarId(int carId);
    }
}
