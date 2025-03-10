using BidManagement.Models;

namespace BidManagement.WinningBidStrategy
{
    public class GenericWinningBidStrategy : IWinningBidStrategy
    {
        public Bid GetWinningBid(IEnumerable<Bid> bids)
        {
            if (bids == null || bids.Count() == 0)
                return null;
            return bids.OrderByDescending(b => b.Amount).FirstOrDefault();

        }
    }
}
