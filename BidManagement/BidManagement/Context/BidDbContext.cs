namespace BidManagement.Context
{
    using BidManagement.Models;
    using Microsoft.EntityFrameworkCore;
    public class BidDbContext : DbContext
    {
        public virtual DbSet<Bid> Bids { get; set; }
        public virtual DbSet<Car> Cars { get; set; }
        public virtual DbSet<Decision> Decisions { get; set; }
        public  BidDbContext(DbContextOptions<BidDbContext> options) : base(options) { }

    }
}
