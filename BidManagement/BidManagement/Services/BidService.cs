using BidManagement.Models;
using BidManagement.Repositories;
using System.Security.Cryptography;

namespace BidManagement.Services
{
    public class BidService : IBidService
    {

        private readonly IBidRepository _bidRepository;
        private readonly IDecisionRepository _decisionRepository;

        public BidService(IBidRepository bidRepository, IDecisionRepository decisionRepository)
        {
            _bidRepository = bidRepository;
            _decisionRepository = decisionRepository;
        }

        public async Task<Decision> GetDecisionByClient(string clientEmail)
        {
          return await _decisionRepository.GetByClientEmailAsync(clientEmail);
        }

        public async Task SaveBidAsync(Bid bid)
        {
            await _bidRepository.SaveBidAsync(bid);
        }

        public async Task SaveBidDecisionAsync(Decision decision)
        {
            await _decisionRepository.SaveDecisionAsync(decision);
        }
    }
}
