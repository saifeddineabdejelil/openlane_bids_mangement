using BidManagement.Context;
using BidManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace BidManagement.Repositories
{
    public class CarRepository : ICarRepository
    {

        private readonly BidDbContext _context;

        public CarRepository(BidDbContext context)
        {
            _context = context;
        }
        public  async Task<Car> GetByCarIdAsync(int carId)
        {
            return await _context.Cars.FindAsync(carId);
        }
    }
}
