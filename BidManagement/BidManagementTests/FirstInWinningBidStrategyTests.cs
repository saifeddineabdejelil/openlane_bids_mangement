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

    public class FirstInWinningBidStrategyTests
    {
        private FirstInWinningBidStrategy _strategy;

        [SetUp]
        public void SetUp()
        {
            _strategy = new FirstInWinningBidStrategy();
        }

        [Test] 
        public void SelectWinningBid_ShouldTheFirstCustomerWithHighestAmount()
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
       
    }
}