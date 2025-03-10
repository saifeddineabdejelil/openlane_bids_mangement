namespace BidManagement.WinningBidStrategy
{
    public class StrategySelector
    {
        private readonly Dictionary<string, IWinningBidStrategy> _brandToStrategyMap;
        public StrategySelector(
        LoyalCustomerWinningBedStrategy loyalCustomerWinningBedStrategy,
        FirstInWinningBidStrategy firstInWinningBidStrategy)
        {
            _brandToStrategyMap = new Dictionary<string, IWinningBidStrategy>
        {
            { "Mercedes", loyalCustomerWinningBedStrategy },
            { "BMW", loyalCustomerWinningBedStrategy },
            { "PEUGEOT", firstInWinningBidStrategy }
        };
        }

        public IWinningBidStrategy GetStrategyForCarBrand(string carBrand)
        {
            if (_brandToStrategyMap.TryGetValue(carBrand, out var strategy))
            {
                return strategy;
            }

            return new GenericWinningBidStrategy();
        }

    }
}
