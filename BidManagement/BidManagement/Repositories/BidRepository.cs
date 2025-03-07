using BidManagement.Context;
using BidManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace BidManagement.Repositories
{
    public class BidRepository : IBidRepository
    {
        private readonly BidDbContext _context;

        public BidRepository(BidDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Bid>> GetBidsByCarId(int carId)
        {
            return await _context.Bids
                            .Where(b => b.CarId == carId)
                            .ToListAsync();
        }

        public async Task<Bid> GetByBidId(int bidId)
        {
            return await _context.Bids.FindAsync(bidId);
        }

        public async Task SaveBidAsync(Bid bid)
        {
            _context.Bids.Add(bid);
            await _context.SaveChangesAsync();
        }
    }
}
