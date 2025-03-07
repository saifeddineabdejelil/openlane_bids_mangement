using BidManagement.Models;

namespace BidManagement.Services
{
    public interface IBidService
    {

        Task SaveBidAsync(Bid bid);

       Task SaveBidDecisionAsync(Decision decision);
        

    }
}
