
using GLMS_API.Strategies;
using NUnit.Framework;

namespace GLMS_Web.Tests
{
    //Code Attribution for NUnit testing framework and unit testing concepts
    //Links:
    //https://www.c-sharpcorner.com/article/introduction-to-nunit-testing-framework/
    //https://docs.nunit.org/articles/nunit/writing-tests/attributes.html

    [TestFixture]
    public class CurrencyCalculationTests
    {
        private UsdToZarStrategy _strategy = null!;

        [SetUp]
        public void Setup()
        {
            _strategy = new UsdToZarStrategy();
        }

        // Tests a normal calculation with valid amount and rate
        [Test]
        public void Calculate_ReturnsCorrectValue()
        {
            var result = _strategy.Calculate(100m, 18.50m);
            Assert.That(result, Is.EqualTo(1850.00m));
        }

        // Tests that zero input returns zero output
        [Test]
        public void Calculate_Zero_ReturnsZero()
        {
            var result = _strategy.Calculate(0m, 18.50m);
            Assert.That(result, Is.EqualTo(0.00m));
        }

        // Tests that a result is correctly rounded to two decimal places
        [Test]
        public void Calculate_RoundsCorrectly()
        {
            var result = _strategy.Calculate(1m, 18.123456m);
            Assert.That(result, Is.EqualTo(18.12m));
        }

        // Tests calculation with a large input value
        [Test]
        public void Calculate_LargeValue_ReturnsCorrect()
        {
            var result = _strategy.Calculate(10000m, 18.50m);
            Assert.That(result, Is.EqualTo(185000.00m));
        }

        // Tests that a rate of 1 returns the original amount
        [Test]
        public void Calculate_RateOne_ReturnsSameValue()
        {
            var result = _strategy.Calculate(250m, 1m);
            Assert.That(result, Is.EqualTo(250.00m));
        }

        // Tests that the PricingContext correctly uses the strategy pattern
        [Test]
        public void PricingContext_UsesStrategyCorrectly()
        {
            var context = new PricingContext(new UsdToZarStrategy());
            var result = context.CalculatePrice(50m, 18.50m);
            Assert.That(result, Is.EqualTo(925.00m));
        }
    }
}