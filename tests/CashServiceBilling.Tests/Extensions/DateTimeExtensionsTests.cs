using CashServiceBilling.Extensions;
using Xunit;

namespace CashServiceBilling.Tests.Extensions;

public class DateTimeExtensionsTests
{
    [Fact]
    public void Now_WhenStubDateTimeIsNull_ReturnsCurrentDateTime()
    {
        // Arrange
        DateTimeExtensions.StubDateTime = null;

        // Act
        var result = DateTimeExtensions.Now;

        // Assert
        Assert.True((DateTime.Now - result).TotalMilliseconds < 1000); // Allow 1 second difference
    }

    [Fact]
    public void Now_WhenStubDateTimeIsSet_ReturnsStubDateTime()
    {
        // Arrange
        var expectedDateTime = new DateTime(2024, 1, 1, 12, 0, 0);
        DateTimeExtensions.StubDateTime = expectedDateTime;

        // Act
        var result = DateTimeExtensions.Now;

        // Assert
        Assert.Equal(expectedDateTime, result);
    }

    [Fact]
    public void StubDateTime_WhenSetAndGet_ReturnsSetValue()
    {
        // Arrange
        var expectedDateTime = new DateTime(2024, 1, 1, 12, 0, 0);

        // Act
        DateTimeExtensions.StubDateTime = expectedDateTime;
        var result = DateTimeExtensions.StubDateTime;

        // Assert
        Assert.Equal(expectedDateTime, result);
    }

    [Fact]
    public void StubDateTime_WhenSetToNull_ReturnsNull()
    {
        // Arrange
        DateTimeExtensions.StubDateTime = new DateTime(2024, 1, 1);

        // Act
        DateTimeExtensions.StubDateTime = null;
        var result = DateTimeExtensions.StubDateTime;

        // Assert
        Assert.Null(result);
    }
}