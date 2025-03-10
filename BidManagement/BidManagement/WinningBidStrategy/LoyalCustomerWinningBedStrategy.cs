using BidManagement.Models;
using BidManagement.Services;
using System.Collections.Generic;

namespace BidManagement.WinningBidStrategy
{
    public class LoyalCustomerWinningBedStrategy : IWinningBidStrategy
    {
        private readonly IBidService _bidService;

        public LoyalCustomerWinningBedStrategy(IBidService bidService)
        {
            _bidService = bidService;
        }

        public Bid GetWinningBid(IEnumerable<Bid> bids)
        {
            if (bids == null || bids.Count() == 0)
                return null;
            return GetLoyalClientBid(bids);
        }


        private Bid GetLoyalClientBid(IEnumerable<Bid> bids)
        {
            var clientBidsEmails = bids.Select(bid => bid.ClientEmail).Distinct().ToList();
            var clientsDecisions = clientBidsEmails.Select(c=>  (_bidService.GetDecisionByClient(c)).Result).ToList();
            var loyalClient ="";
            int maxWinningBids = -1;

            foreach (var clientEmail in clientBidsEmails)
            {
                var winningBidCount = clientsDecisions.Where(cd=>cd  != null).Count(decision => decision.Bid.ClientEmail == clientEmail && decision.IsWinning);

                if (winningBidCount > maxWinningBids)
                {
                    loyalClient = clientEmail;
                    maxWinningBids = winningBidCount;
                }
               
            }
                return bids
                    .Where(bid => bid.ClientEmail == loyalClient)
                    .FirstOrDefault();
            
        }
    }
}
