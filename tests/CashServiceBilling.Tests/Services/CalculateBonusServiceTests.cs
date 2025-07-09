using System.Text.Json;
using CashServiceBilling.Criteria;
using CashServiceBilling.Models;
using CashServiceBilling.Services;
using Fanex.Data.Repository;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CashServiceBilling.Tests.Services;

public class CalculateBonusServiceTests
{
    private readonly Mock<IDynamicRepository> repositoryMock;
    private readonly Mock<Fanex.Logging.ILogger> loggerMock;
    private readonly Mock<IConfiguration> mockConfiguration;
    private readonly Mock<IConfigurationSection> mockConfigurationSection;
    private readonly CalculateBonusService promotionService;

    public CalculateBonusServiceTests()
    {
        repositoryMock = new Mock<IDynamicRepository>();
        loggerMock = new Mock<Fanex.Logging.ILogger>();
        mockConfiguration = new Mock<IConfiguration>();
        mockConfigurationSection = new Mock<IConfigurationSection>();

        mockConfigurationSection.Setup(x => x.Value).Returns("TestCategory");
        mockConfiguration.Setup(x => x.GetSection("JobsSettings:NotifyCategory")).Returns(mockConfigurationSection.Object);
        // Replace GetValue with GetSection().Value since GetValue is an extension method
        mockConfiguration.Setup(x => x.GetSection("JobsSettings:NotifyCategory").Value).Returns("TestCategory");

        promotionService = new CalculateBonusService(
            repositoryMock.Object,
            loggerMock.Object,
            mockConfiguration.Object);
    }

    [Fact]
    public async Task AddBonusTransaction_ShouldLogStartAndComplete()
    {
        // Arrange
        mockConfiguration.Setup(x => x.GetSection("JobsSettings:NotifyCategory").Value).Returns("TestCategory");
        SetupGetAgentSettingPromotionToReturnEmptyJson();

        // Act
        await promotionService.AddBonusTransaction(false, 100);

        // Assert
        loggerMock.Verify(l => l.ErrorAsync(It.Is<string>(s => s.Contains("Start [Add pending promotion]")), "TestCategory"), Times.Once);
        loggerMock.Verify(l => l.ErrorAsync(It.Is<string>(s => s.Contains("[Add pending promotion] Agent Setting Promotion Count: 0")), "TestCategory"), Times.Once);
        loggerMock.Verify(l => l.ErrorAsync(It.Is<string>(s => s.Contains("[Add pending promotion] completed")), "TestCategory"), Times.Once);
    }

    [Fact]
    public async Task GetAgentSettingPromotion_ShouldExecuteCorrectCriteria()
    {
        // Arrange
        const bool isTest = true;
        const string expectedJson = "test json";

        repositoryMock.Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
            .Callback<INonQueryCommand>(criteria =>
            {
                var c = criteria as GetAgentSettingBonusCriteria;
                c.AgentPromotion = expectedJson;
            })
            .Returns(Task.FromResult(0));

        // Act
        var result = await promotionService.GetAgentSettingPromotion(isTest);

        // Assert
        repositoryMock.Verify(r => r.ExecuteAsync(It.Is<GetAgentSettingBonusCriteria>(c => c.IsTest == isTest)), Times.Once);
        Assert.Equal(expectedJson, result);
    }

    [Fact]
    public async Task AddBonusTransaction_WithValidData_ShouldProcessPromotions()
    {
        // Arrange
        mockConfiguration.Setup(x => x.GetSection("JobsSettings:NotifyCategory").Value).Returns("TestCategory");
        var promotions = new[]
        {
                new AgentSettingPromotion { SiteDBGroupId = 1 },
                new AgentSettingPromotion { SiteDBGroupId = 1 },
                new AgentSettingPromotion { SiteDBGroupId = 2 }
            };

        var json = JsonSerializer.Serialize(promotions);
        SetupGetAgentSettingPromotionToReturn(json);

        repositoryMock.Setup(r => r.ExecuteAsync(It.IsAny<AddBonusTransactionCriteria>()))
            .Returns(Task.FromResult(0));

        // Act
        await promotionService.AddBonusTransaction(false, 10);

        // Assert
        repositoryMock.Verify(r => r.ExecuteAsync(It.Is<AddBonusTransactionCriteria>(
            c => c.SiteDBGroupId == 1)), Times.Once);

        repositoryMock.Verify(r => r.ExecuteAsync(It.Is<AddBonusTransactionCriteria>(
            c => c.SiteDBGroupId == 2)), Times.Once);
    }

    [Fact]
    public async Task AddBonusTransaction_WithChunking_ShouldCreateMultipleBatches()
    {
        // Arrange
        const short siteDbGroupId = 1;
        const int batchSize = 2;

        var promotions = new[]
        {
                new AgentSettingPromotion { SiteDBGroupId = siteDbGroupId },
                new AgentSettingPromotion { SiteDBGroupId = siteDbGroupId },
                new AgentSettingPromotion { SiteDBGroupId = siteDbGroupId },
                new AgentSettingPromotion { SiteDBGroupId = siteDbGroupId },
                new AgentSettingPromotion { SiteDBGroupId = siteDbGroupId }
            };

        // Act
        await promotionService.AddBonusTransaction(promotions, siteDbGroupId, batchSize);

        // Assert
        // We expect 3 batches: 2 items, 2 items, and 1 item
        repositoryMock.Verify(r => r.ExecuteAsync(It.IsAny<AddBonusTransactionCriteria>()), Times.Exactly(3));
    }

    [Fact]
    public async Task AddBonusTransaction_WithEmptyBatch_ShouldExecuteCriteria()
    {
        // Arrange
        const short siteDbGroupId = 1;
        var promotions = new List<AgentSettingPromotion>();

        // Act
        await promotionService.AddBonusTransaction(promotions, siteDbGroupId);

        // Assert
        repositoryMock.Verify(r => r.ExecuteAsync(It.Is<AddBonusTransactionCriteria>(
            c => c.SiteDBGroupId == siteDbGroupId)), Times.Once);
    }

    private void SetupGetAgentSettingPromotionToReturnEmptyJson()
    {
        SetupGetAgentSettingPromotionToReturn(string.Empty);
    }

    private void SetupGetAgentSettingPromotionToReturn(string json)
    {
        repositoryMock.Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
            .Callback<INonQueryCommand>(criteria =>
            {
                if (criteria is GetAgentSettingBonusCriteria)
                {
                    var c = criteria as GetAgentSettingBonusCriteria;
                    c.AgentPromotion = json;
                }
            })
            .Returns(Task.FromResult(0));
    }
}
