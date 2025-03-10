using BidManagement.Context;
using BidManagement.Models;
using BidManagement.Repositories;
using BidManagement.Services;
using BidManagement.WinningBidStrategy;
using Microsoft.EntityFrameworkCore;

namespace BidManagementTests
{
    [TestFixture]

    public class BidRepositoryTest
    {
        private IBidService bidService;
        private IBidRepository _bidRepository;
        private IDecisionRepository _decisionRepository;
        private DbContextOptions<BidDbContext> _options;
        private BidDbContext _context;



        [SetUp]
        public void SetUp()
        {

            _options = new DbContextOptionsBuilder<BidDbContext>()
            .UseInMemoryDatabase(databaseName: "BidDbTest")
            .Options;

            _context = new BidDbContext(_options);

            _bidRepository = new BidRepository(_context);
            _decisionRepository = new DecisionRepository(_context);
            bidService = new BidService(_bidRepository, _decisionRepository);
        }
        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }
        [Test]
        public async Task SaveBidAsync_ShouldSaveBidSuccessfully()
        {
            var bid = new Bid
            {
                Id = 2,
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
                Id= 1,
                CarId = 3,
                Amount = 5000,
                ClientEmail = "test@test.com",
                BidTime = DateTime.Now.AddMinutes(1)
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