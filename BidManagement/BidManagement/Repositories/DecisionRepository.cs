using BidManagement.Context;
using BidManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace BidManagement.Repositories
{
    public class DecisionRepository : IDecisionRepository
    {
        private readonly BidDbContext _context;

        public DecisionRepository(BidDbContext context)
        {
            _context = context;
        }
        public async Task<Decision> GetByBidIdAsync(int bidId)
        {
            return await _context.Decisions
                           .FirstOrDefaultAsync(bd => bd.BidId == bidId);
        }
    

        public async Task<Decision> GetByCarIdAsync(int carId)
        {
            return await _context.Decisions
                           .FirstOrDefaultAsync(bd => bd.CarId == carId);
        }
    

        public async Task<Decision> GetByClientEmailAsync(string email)
        {
            return await _context.Decisions
                            .FirstOrDefaultAsync(bd => bd.Bid.ClientEmail == email);
        }

        public async Task SaveDecisionAsync(Decision decision)
        {
            _context.Decisions.Add(decision);
            await _context.SaveChangesAsync();
        }
    }
}
