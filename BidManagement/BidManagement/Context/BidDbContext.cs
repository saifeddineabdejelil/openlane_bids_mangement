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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Car>()
                .Property(c => c.Price)
                .HasColumnType("DECIMAL(18,2)");
            modelBuilder.Entity<Bid>()
                .Property(d => d.Amount)
                .HasColumnType("DECIMAL(18,2)");
            modelBuilder.Entity<Car>().HasData(
            new Car
            {
                Id = 1,
                Model = "Peugeot",
                Price = 1000.99m,
                ClientEmail = "Peugeot@example.com",
            },
            new Car
            {
                Id = 2,
                Model = "BMW",
                Price = 50000.99m,
                ClientEmail = "bmw@example.com",
            },
            new Car
            {
                Id = 3,
                Model = "Mercedes",
                Price = 45000.00m,
                ClientEmail = "Mercedes@example.com",
            });
       
            base.OnModelCreating(modelBuilder);
        }

    }
}
