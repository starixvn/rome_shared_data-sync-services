using ResetPotentialWinLimit.Extensions;
using Xunit;

namespace ResetPotentialWinLimit.Tests.Extensions;

public class DateTimeExtentionsTests
{
    [Fact]
    public void CurrentDateTime_WhenStubDateTimeIsNull_ReturnsCurrentDateTime()
    {
        // Arrange
        DateTimeExtentions.StubDateTime = null;
        
        // Act
        var result = DateTimeExtentions.Now;
        
        // Assert
        Assert.True((DateTime.Now - result).TotalSeconds < 1);
    }

    [Fact]
    public void CurrentDateTime_WhenStubDateTimeIsSet_ReturnsStubDateTime()
    {
        // Arrange
        var expectedDateTime = new DateTime(2024, 1, 1, 12, 0, 0);
        DateTimeExtentions.StubDateTime = expectedDateTime;
        
        // Act
        var result = DateTimeExtentions.Now;
        
        // Assert
        Assert.Equal(expectedDateTime, result);
    }

    [Theory]
    [InlineData("2024-01-01 12:00:00", "2024-01-01 15:30:00", true)]  // Same day, different times
    [InlineData("2024-01-01 12:00:00", "2024-01-02 12:00:00", false)] // Different days
    [InlineData("2024-01-01 12:00:00", "2024-02-01 12:00:00", false)] // Different months
    [InlineData("2024-01-01 12:00:00", "2025-01-01 12:00:00", false)] // Different years
    public void IsSameDate_WhenComparingDates_ReturnsExpectedResult(string date1, string date2, bool expected)
    {
        // Arrange
        var dateTime1 = DateTime.Parse(date1);
        var dateTime2 = DateTime.Parse(date2);
        
        // Act
        var result = dateTime1.IsSameDate(dateTime2);
        
        // Assert
        Assert.Equal(expected, result);
    }
} 