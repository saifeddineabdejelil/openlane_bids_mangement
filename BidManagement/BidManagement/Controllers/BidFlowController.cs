using BidManagement.Models;
using BidManagement.Services;
using BidManagement.Utilis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client.Exceptions;

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

        [HttpPost(Name = "receive")]
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
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, $"Argument null exception while processing bid (ID: {bid?.Id}).");
                return BadRequest("Invalid bid data: Required fields are missing.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, $"Invalid operation exception while processing bid (ID: {bid?.Id}).");
                return BadRequest("Invalid operation: The bid could not be processed.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Database error while saving bid (ID: {bid?.Id}).");
                return StatusCode(500, "An error occurred while saving the bid.");
            }
            catch (BrokerUnreachableException ex)
            {
                _logger.LogError(ex, $"RabbitMQ connection error while publishing bid (ID: {bid?.Id}).");
                return StatusCode(500, "An error occurred while publishing the bid.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error while processing bid (ID: {bid?.Id}).");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }
    }
}