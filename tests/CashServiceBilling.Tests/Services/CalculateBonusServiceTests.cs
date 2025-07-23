using System.Text.Json;
using CashServiceBilling.Criteria;
using CashServiceBilling.Models;
using CashServiceBilling.Services;
using Fanex.Data.Repository;
using Microsoft.Extensions.Configuration;
using Moq;
using Shared.Configuration;
using Shared.Services;

namespace CashServiceBilling.Tests.Services;

public class CalculateBonusServiceTests
{
    private readonly Mock<IDynamicRepository> _repositoryMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly CalculateBonusService _service;

    public CalculateBonusServiceTests()
    {
        _repositoryMock = new Mock<IDynamicRepository>();
        _loggerMock = new Mock<ILoggerService>();
        _service = new CalculateBonusService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CalculateAndAddBonusTransactions_NoAgentPromotions_ShouldNotCalculateOrAddBonusTransactions()
    {
        // Arrange
        var jobsSettings = new JobsSettingsModel { NotifyCategory = "TestCategory", IsTest = true, SiteDBGroupIds = new List<int> { 1 } };
        var scheduleJob = new ScheduleJob();

        _repositoryMock
            .Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
            .Callback<INonQueryCommand>(c =>
            {
                var criteria = c as GetAgentSettingBonusCriteria;
                criteria.AgentPromotion = null;
            })
            .Returns(Task.FromResult(0));

        // Act
        await _service.CalculateAndAddBonusTransactions(jobsSettings, scheduleJob);

        // Assert
        _loggerMock.Verify(l => l.Notify(It.Is<string>(s => s.StartsWith("Start")), jobsSettings.NotifyCategory), Times.Once);
        _loggerMock.Verify(l => l.Notify(It.Is<string>(s => s.Contains("AgentSettingBonus Count: 0")), jobsSettings.NotifyCategory), Times.Once);
        _loggerMock.Verify(l => l.Notify(It.Is<string>(s => s.EndsWith("completed.")), jobsSettings.NotifyCategory), Times.Once);
        _repositoryMock.Verify(r => r.ExecuteAsync(It.IsAny<CalculateCashPromotionCriteria>()), Times.Never);
    }

    [Fact]
    public async Task CalculateAndAddBonusTransactions_WithAgentPromotions_ShouldCalculateAndAddBonusTransactions()
    {
        // Arrange
        var jobsSettings = new JobsSettingsModel { NotifyCategory = "TestCategory", IsTest = true, SiteDBGroupIds = new List<int> { 1 } };
        var scheduleJob = new ScheduleJob { Parameters = Array.Empty<ScheduleJobParameter>() };

        var agentPromotions = new[] { new AgentPromotion { AgentId = 1 } };

        _repositoryMock
            .Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
            .Callback<INonQueryCommand>(c =>
            {
                if (c is GetAgentSettingBonusCriteria criteria)
                {
                    criteria.AgentPromotion = JsonSerializer.Serialize(agentPromotions);
                }
                else if (c is GetBonusTransactionCriteria getCriteria)
                {
                    getCriteria.MaxCustId = 0;
                    getCriteria.MemberBonus = null;
                }
            })
            .Returns(Task.FromResult(0));

        _repositoryMock
            .Setup(r => r.GetAsync<BalanceUpDayAndMoveBetsDay>(It.IsAny<GetBalanceUpDayAndMoveBetsDayCriteria>()))
            .ReturnsAsync(new BalanceUpDayAndMoveBetsDay { BalanceUpDay = DateTime.Now, MoveBetsDay = DateTime.Now });

        _repositoryMock
            .Setup(r => r.FetchAsync<ProductDetail>(It.IsAny<GetProductsDetailCriteria>()))
            .ReturnsAsync(new List<ProductDetail>());

        // Act
        await _service.CalculateAndAddBonusTransactions(jobsSettings, scheduleJob);

        // Assert
        _repositoryMock.Verify(r => r.ExecuteAsync(It.IsAny<CalculateCashPromotionCriteria>()), Times.Once);
        _repositoryMock.Verify(r => r.ExecuteAsync(It.IsAny<GetBonusTransactionCriteria>()), Times.AtLeastOnce);
        _loggerMock.Verify(l => l.Notify(It.Is<string>(s => s.Contains("AgentSettingBonus Count: 1")), jobsSettings.NotifyCategory), Times.Once);
    }

    [Fact]
    public void GetMondaySundayLastWeek_ShouldReturnCorrectDates()
    {
        // Arrange
        var today = new DateTime(2023, 8, 22); // Tuesday
        var expectedMonday = new DateTime(2023, 8, 14); // Monday of last week
        var expectedSunday = new DateTime(2023, 8, 20); // Sunday of last week

        // Act
        var (mondayLastWeek, sundayLastWeek) = CalculateBonusService.GetMondaySundayLastWeek();

        // Assert
        Assert.Equal(DayOfWeek.Monday, mondayLastWeek.DayOfWeek);
        Assert.Equal(DayOfWeek.Sunday, sundayLastWeek.DayOfWeek);
        Assert.True(mondayLastWeek < sundayLastWeek);
        Assert.Equal(6, (sundayLastWeek - mondayLastWeek).Days);
    }

    [Fact]
    public async Task GetAgentSettingBonus_WhenDataExists_ShouldReturnDeserializedData()
    {
        // Arrange
        var agentPromotions = new[] { new AgentPromotion { AgentId = 1 } };
        var serializedData = JsonSerializer.Serialize(agentPromotions);

        _repositoryMock
            .Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
            .Callback<INonQueryCommand>(c =>
            {
                var criteria = c as GetAgentSettingBonusCriteria;
                criteria.AgentPromotion = serializedData;
            })
            .Returns(Task.FromResult(0));

        // Act
        var result = await _service.GetAgentSettingBonus(false);

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0].AgentId);
    }

    [Fact]
    public async Task GetAgentSettingBonus_WhenNoData_ShouldReturnEmptyArray()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
            .Callback<INonQueryCommand>(c =>
            {
                var criteria = c as GetAgentSettingBonusCriteria;
                criteria.AgentPromotion = null;
            })
            .Returns(Task.FromResult(0));

        // Act
        var result = await _service.GetAgentSettingBonus(false);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("BatchSize", "100", 100)]
    [InlineData("batchsize", "50", 50)]
    [InlineData("OtherParam", "100", 3000)]
    [InlineData("BatchSize", "invalid", 3000)]
    public void GetBatchSize_ShouldReturnCorrectValue(string paramKey, string paramValue, int expectedBatchSize)
    {
        // Arrange
        var scheduleJob = new ScheduleJob
        {
            Parameters = new[]
            {
                new ScheduleJobParameter
                {
                    Key = paramKey,
                    Value = paramValue
                }
            }
        };

        // Act
        var batchSize = CalculateBonusService.GetBatchSize(scheduleJob);

        // Assert
        Assert.Equal(expectedBatchSize, batchSize);
    }

    [Fact]
    public async Task CalculateCashPromotions_ShouldHandleExceptions()
    {
        // Arrange
        var scheduleJob = new ScheduleJob();

        _repositoryMock
            .Setup(r => r.ExecuteAsync(It.IsAny<CalculateCashPromotionCriteria>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        await _service.CalculateCashPromotions("xml", DateTime.Now, DateTime.Now, "{}", scheduleJob);

        // Assert
        _loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), scheduleJob, nameof(CalculateBonusService.CalculateCashPromotions), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AddBonusTransactionsFlow_ShouldProcessAllTransactions()
    {
        // Arrange
        var siteDBGroupIds = new List<int> { 1 };
        var memberBonus = "test_bonus_data";

        var callCount = 0;
        _repositoryMock
            .Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
            .Callback<INonQueryCommand>(c =>
            {
                if (c is GetBonusTransactionCriteria)
                {
                    var criteria = c as GetBonusTransactionCriteria;
                    callCount++;
                    if (callCount == 1)
                    {
                        criteria.MaxCustId = 10;
                        criteria.MemberBonus = memberBonus;
                    }
                    else
                    {
                        criteria.MaxCustId = 0;
                        criteria.MemberBonus = null;
                    }
                }
            })
            .Returns(Task.FromResult(0));

        // Act
        await _service.AddBonusTransactionsFlow(siteDBGroupIds);

        // Assert
        _repositoryMock.Verify(r => r.ExecuteAsync(It.IsAny<GetBonusTransactionCriteria>()), Times.Exactly(2));
        _repositoryMock.Verify(r => r.ExecuteAsync(It.IsAny<AddBonusTransactionCriteria>()), Times.Once);
    }
}