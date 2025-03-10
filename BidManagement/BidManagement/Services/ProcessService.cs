using BidManagement.Context;
using BidManagement.Controllers;
using BidManagement.Models;
using BidManagement.WinningBidStrategy;
using Microsoft.EntityFrameworkCore;

namespace BidManagement.Services
{
    public class ProcessService :BackgroundService
    {
        private readonly IQueueService _queueService;
        private readonly BidDbContext _context;
        private readonly StrategySelector _strategySelector;
        private readonly IEmailSenderService _emailSenderService;
        private readonly ILogger<BidFlowController> _logger;

        private string carBrand = "";
        public ProcessService(IQueueService queueService, BidDbContext context,
            StrategySelector strategySelector, IEmailSenderService emailSenderService, ILogger<BidFlowController> logger)
        {
            _queueService = queueService;
            _context = context;
            _strategySelector = strategySelector;
            _emailSenderService = emailSenderService;
            _logger = logger;
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

                    await SaveDecision(winningBid, carBrand);

                    await SendEmailNotificationAsync(winningBid, carBrand);
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
            var carId = bids.First().CarId;
             carBrand = _context.Cars.First(c => c.Id == carId).Model;
            var strategy = _strategySelector.GetStrategyForCarBrand(carBrand);

            var winningBid = strategy.GetWinningBid(bids);
            var decision = _context.Decisions.FirstOrDefault(b => b.BidId == winningBid.Id);

            return winningBid;
        }

        private async Task SaveDecision(Bid winningBid, string carBrand)
        {
            try
            {
                var decision = new Decision
                {
                    CarId = winningBid.CarId,
                    BidId = winningBid.Id,
                    IsWinning = true
                };

                _context.Decisions.Add(decision);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error when saving decision");

            }

        }
        private async Task SendEmailNotificationAsync(Bid winningBid, string carbrand)
        {
            try
            {
            var clientEmail = winningBid.ClientEmail;
            var subject = "Congratulations! Your bid was successful";
            var body = $"Dear client,<br>" +
                       $"<b>Congratulations!</b><br>" +
                       $"You have won the bid for the car {carbrand} with a bid of {winningBid.Amount:C}.<br>" +
                       $"Thank you for bidding with us!<br><br>" +
                       $"We will contact you for the rest of procedure<br><br>" +
                       $"Best regards,<br>Openlane";

            await _emailSenderService.SendBidWinningEmailAsync(clientEmail, subject, body);
            }
            catch (Exception ex )
            {
                _logger.LogError(ex, "error sending email");

            }

        }
    }
}
