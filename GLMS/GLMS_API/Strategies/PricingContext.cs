namespace GLMS_API.Strategies
{
    // Context class for executing pricing strategies
    public class PricingContext
    {
        private readonly IPriceStrategy _priceStrategy;

        public PricingContext(IPriceStrategy priceStrategy)
        {
            _priceStrategy = priceStrategy;
        }

        // Executes the selected pricing strategy
        public decimal CalculatePrice(decimal amount, decimal rate)
        {
            return _priceStrategy.Calculate(amount, rate);
        }
    }
}