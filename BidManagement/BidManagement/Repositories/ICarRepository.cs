using BidManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace BidManagement.Repositories
{
    public interface ICarRepository
    {
          Task<Car> GetByCarIdAsync(int carId);
       
    }
}
