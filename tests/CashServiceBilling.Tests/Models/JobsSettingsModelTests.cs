using CashServiceBilling.Models;

namespace CashServiceBilling.Tests.Models;

public class JobsSettingsModelTests
{
    [Fact]
    public void JobsSettingsModel_Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var model = new JobsSettingsModel
        {
            NotifyCategory = "TestCategory",
            SiteDBGroupIds = new[] { 1, 2, 3 },
            AccumulatedCreditJobCron = "0 0 * * *",
            CalculateBonusJobCron = "0 12 * * *",
            IsTest = true
        };

        // Act & Assert
        Assert.Equal("TestCategory", model.NotifyCategory);
        Assert.Equal(new[] { 1, 2, 3 }, model.SiteDBGroupIds);
        Assert.Equal("0 0 * * *", model.AccumulatedCreditJobCron);
        Assert.Equal("0 12 * * *", model.CalculateBonusJobCron);
        Assert.True(model.IsTest);
    }

    [Fact]
    public void JobsSettingsModel_DefaultConstructor_CreatesInstance()
    {
        // Act
        var model = new JobsSettingsModel();

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void JobsSettingsModel_SiteDBGroupIds_CanBeEmpty()
    {
        // Arrange
        var model = new JobsSettingsModel
        {
            SiteDBGroupIds = Enumerable.Empty<int>()
        };

        // Act & Assert
        Assert.Empty(model.SiteDBGroupIds);
    }

    [Fact]
    public void JobsSettingsModel_SiteDBGroupIds_CanBeModified()
    {
        // Arrange
        var model = new JobsSettingsModel
        {
            SiteDBGroupIds = new[] { 1, 2, 3 }
        };

        // Act
        var originalIds = model.SiteDBGroupIds;
        model.SiteDBGroupIds = new[] { 4, 5 };

        // Assert
        Assert.Equal(new[] { 4, 5 }, model.SiteDBGroupIds);
        Assert.NotEqual(originalIds, model.SiteDBGroupIds);
    }
}
