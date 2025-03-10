using BidManagement.Models;
using BidManagement.Services;
using BidManagement.Utilis;
using Microsoft.AspNetCore.Mvc;

namespace BidManagement.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BidFlowController : ControllerBase
    {

        private readonly ILogger<BidFlowController> _logger;
        private readonly IBidService _bidService;
        private readonly IQueueService _queueService;

        public BidFlowController(IBidService bidService, ILogger<BidFlowController> logger, IQueueService queueService)
        {
            _bidService = bidService;
            _logger = logger;
            _queueService = queueService;
        }

        [HttpGet(Name = "receive")]
        public async Task<IActionResult> ReceiveBid([FromBody] Bid bid)
        {
            string validationMessage;
            if (!BidValidator.IsValid(bid, out validationMessage))
            {
                return BadRequest(validationMessage);
            }
            try
            {
                await _bidService.SaveBidAsync(bid);

                _logger.LogInformation($"Bid  ( {bid.Id} ) saved successfully.");

                _queueService.PublishBid(bid);
                _logger.LogInformation($"Bid  ( {bid.Id} ) pushed in queue successfully.");

                return Ok("Success reception of bid");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error in bid recpetion");
                return BadRequest("error");
            }

        }
    }
}
