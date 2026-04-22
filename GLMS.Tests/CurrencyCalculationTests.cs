using Xunit;
using System;

namespace GLMS.Tests
{
    public class CurrencyCalculationTests
    {
        [Fact]
        public void ConvertUsdToZar_ValidAmount_ReturnsCorrectConversion()
        {
            // Arrange
            decimal usdAmount = 100.00m;
            decimal expectedRate = 16.50m;

            // Act
            decimal zarAmount = usdAmount * expectedRate;

            // Assert
            Assert.True(zarAmount > 0, "ZAR amount should be greater than zero");
            Assert.Equal(1650.00m, zarAmount);
        }

        [Fact]
        public void ConvertUsdToZar_ZeroAmount_ReturnsZero()
        {
            // Arrange
            decimal usdAmount = 0;
            decimal rate = 16.50m;

            // Act
            decimal zarAmount = usdAmount * rate;

            // Assert
            Assert.Equal(0, zarAmount);
        }

        [Fact]
        public void ConvertUsdToZar_NegativeAmount_ReturnsNegativeValue()
        {
            // Arrange
            decimal usdAmount = -50.00m;
            decimal rate = 16.50m;

            // Act
            decimal zarAmount = usdAmount * rate;

            // Assert
            Assert.True(zarAmount < 0, "Negative USD amount should produce negative ZAR amount");
            Assert.Equal(-975.00m, zarAmount);
        }

        [Theory]
        [InlineData(1, 16.50)]
        [InlineData(10, 165.00)]
        [InlineData(100, 1650.00)]
        [InlineData(1000, 16500.00)]
        [InlineData(123.45, 2407.275)]
        public void CurrencyConversion_WithVariousAmounts_CalculatesCorrectly(decimal usdAmount, decimal expectedZar)
        {
            // Arrange
            decimal rate = 16.50m;

            // Act
            decimal calculatedZar = usdAmount * rate;

            // Assert
            Assert.Equal(expectedZar, calculatedZar, 3);
        }

        [Fact]
        public void ExchangeRate_IsWithinReasonableRange()
        {
            // Arrange
            decimal rate = 16.50m;

            // Assert
            Assert.InRange(rate, 15, 25);
        }

        [Fact]
        public void CurrencyConversion_MultipliesCorrectly()
        {
            // Arrange
            decimal usdAmount = 250.00m;
            decimal rate = 16.50m;
            decimal expectedZar = 4875.00m;

            // Act
            decimal actualZar = usdAmount * rate;

            // Assert
            Assert.Equal(expectedZar, actualZar);
        }
    }
}