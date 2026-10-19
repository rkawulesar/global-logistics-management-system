namespace GLMS_API.Strategies
{
    public class UsdToZarStrategy : IPriceStrategy
    {
        // Converts and rounds to 2 decimal places
        public decimal Calculate(decimal amount, decimal rate)
        {
            return Math.Round(amount * rate, 2);
        }
    }
}