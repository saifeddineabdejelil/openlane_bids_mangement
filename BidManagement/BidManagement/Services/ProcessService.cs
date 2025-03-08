using BidManagement.Context;
using BidManagement.Models;

namespace BidManagement.Services
{
    public class ProcessService :BackgroundService
    {
        private readonly IQueueService _queueService;
        private readonly BidDbContext _context;

        public ProcessService(IQueueService queueService, BidDbContext context)
        {
            _queueService = queueService;
            _context = context;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await _queueService.InitializeAsync();

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var bids = await GetAllBidsAsync(stoppingToken);


                var winningBid = GetWinningBid(bids);
              
            }
        }
        private async Task<List<Bid>> GetAllBidsAsync(CancellationToken stoppingToken)
        {
            var bids = new List<Bid>();

            while (!stoppingToken.IsCancellationRequested)
            {
                var bid = await _queueService.DequeueBidAsync(stoppingToken);
                if (bid != null)
                {
                    bids.Add(bid);
                }
                else
                {
                    break;
                }
            }

            return bids;
        }

        private Bid GetWinningBid(List<Bid> bids)
        {
            if (bids == null || bids.Count == 0)
            {
                return null;  
            }

            var winningBid = bids.OrderByDescending(b => b.Amount).FirstOrDefault();
            return winningBid;
        }

    }
}
