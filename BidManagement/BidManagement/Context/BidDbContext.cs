namespace BidManagement.Context
{
    using BidManagement.Models;
    using Microsoft.EntityFrameworkCore;
    public class BidDbContext : DbContext
    {
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Decision> Decisions { get; set; }
        public BidDbContext(DbContextOptions<BidDbContext> options) : base(options) { }

    }
}
