namespace BidManagement.WinningBidStrategy
{
    public class StrategySelector
    {
        private readonly Dictionary<string, IWinningBidStrategy> _strategies;

        public StrategySelector(Dictionary<string, IWinningBidStrategy> strategies)
        {
            _strategies = strategies;
        }

        public IWinningBidStrategy GetStrategy(string carType)
        {
            if (_strategies.TryGetValue(carType, out var strategy))
            {
                return strategy;
            }
            return _strategies["Generic"]; 
        }
    }
}