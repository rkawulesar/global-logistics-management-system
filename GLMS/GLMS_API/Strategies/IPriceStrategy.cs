namespace GLMS_API.Strategies
{
    // Defines the contract for pricing strategies 
    public interface IPriceStrategy
    {
        // Calculates the price based on amount and rate
        decimal Calculate(decimal amount, decimal rate);
    }
}