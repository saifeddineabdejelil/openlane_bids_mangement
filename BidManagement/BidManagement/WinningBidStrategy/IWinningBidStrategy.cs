using BidManagement.Models;

namespace BidManagement.WinningBidStrategy
{
    public interface IWinningBidStrategy
    {
        Bid GetWinningBid(IEnumerable<Bid> bids);
    }
}
