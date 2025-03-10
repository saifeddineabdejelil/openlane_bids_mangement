using BidManagement.Models;
using BidManagement.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using BidManagement.WinningBidStrategy;
using BidManagement.Services;
using BidManagement.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BidManagementTests
{
    [TestFixture]

    public class LoyalCustomerWinningBedStrategyTests
    {
        private LoyalCustomerWinningBedStrategy _strategy;
        private IBidService  bidService;
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


            _context.Decisions.AddRange(new List<Decision>
            {
                     new Decision {Id=1, BidId = 1, CarId = 1, IsWinning = true , Bid = new Bid { Id = 1, ClientEmail = "client1@example.com", Amount = 100, BidTime = DateTime.Now.AddDays(-1) } },
                    new Decision {Id=2, BidId = 2, CarId = 2, IsWinning = true, Bid = new Bid { Id = 2, ClientEmail = "client2@example.com", Amount = 200, BidTime = DateTime.Now.AddDays(-1) } },
                    new Decision {Id=3, BidId = 3, CarId = 3, IsWinning = true, Bid = new Bid { Id = 3, ClientEmail = "client1@example.com", Amount = 300, BidTime = DateTime.Now.AddDays(-1) } }
            });


            _context.SaveChanges();
            _bidRepository = new BidRepository( _context );
            _decisionRepository = new DecisionRepository(_context);
            bidService = new BidService(_bidRepository, _decisionRepository);
            _strategy = new LoyalCustomerWinningBedStrategy(bidService);
        }

        [Test] 
        public void SelectWinningBid_ShouldPickLoyalClientWithMostWins()
        {
           var bids = new List<Bid>
            {
                new Bid { Id = 4, ClientEmail = "client1@example.com", Amount = 450, BidTime = DateTime.Now.AddMinutes(-2) }
    ,               new Bid { Id = 5, ClientEmail = "client3@example.com", Amount = 300, BidTime = DateTime.Now.AddMinutes(-1) }
    ,               new Bid { Id = 6, ClientEmail = "client4@example.com", Amount = 450, BidTime = DateTime.Now }

            };

            var winningBid = _strategy.GetWinningBid(bids); 

            Assert.NotNull(winningBid);
            Assert.AreEqual("client1@example.com", winningBid.ClientEmail);
        }
        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }
    }
}