using BidManagement.Context;
using BidManagement.Models;
using BidManagement.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BidManagementTests
{
    [TestFixture]

    public class BidRepositoryTest
    {
        private BidDbContext _context;
        private IBidRepository _bidRepository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<BidDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new BidDbContext(options);
            _bidRepository = new BidRepository(_context);
        }
        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
        [Test]
        public async Task SaveBidAsync_ShouldSaveBidSuccessfully()
        {
            var bid = new Bid
            {
                CarId = 33,
                Amount = 5000,
                ClientEmail = "test@test.com",
                BidTime = DateTime.Now
            };

            await _bidRepository.SaveBidAsync(bid);
            var savedBid = await _context.Bids.FindAsync(bid.Id);
            Assert.IsNotNull(savedBid);
            Assert.AreEqual(bid.Amount, savedBid.Amount);
        }
    

        [Test]
        public async Task GetBidByIdAsync_ShouldReturnBid()
        {
            var bid = new Bid
            {
                CarId = 33,
                Amount = 5000,
                ClientEmail = "test@test.com",
                BidTime = DateTime.Now
            };

            await _bidRepository.SaveBidAsync(bid);

            var result = await _bidRepository.GetByBidId(bid.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(5000, result.Amount);
        }

        [Test]
        public async Task GetBidByIdAsync_ShouldReturnNull_WhenBidNotFound()
        {
            var fetchedBid = await _bidRepository.GetByBidId(20); 

            Assert.IsNull(fetchedBid);
        }
    }
}