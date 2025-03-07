using BidManagement.Models;

namespace BidManagement.Repositories
{
    public interface IDecisionRepository
    {
        Task SaveDecisionAsync(Decision decision);
        Task<Decision> GetByCarIdAsync(int id);
        Task<Decision> GetByBidIdAsync(int id);
        Task<Decision> GetByClientEmailAsync(string email);
    }
}
