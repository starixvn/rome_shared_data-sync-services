using System.Text.Json;
using CashServiceBilling.Criteria;
using CashServiceBilling.Models;
using CashServiceBilling.Services;
using Fanex.Data.Repository;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CashServiceBilling.Tests.Services
{
    public class CalculateBonusServiceTest
    {
        private readonly Mock<IDynamicRepository> repositoryMock;
        private readonly Mock<Fanex.Logging.ILogger> loggerMock;
        private readonly Mock<IConfiguration> configurationMock;
        private readonly CalculateBonusService service;

        public CalculateBonusServiceTest()
        {
            repositoryMock = new Mock<IDynamicRepository>();
            loggerMock = new Mock<Fanex.Logging.ILogger>();
            configurationMock = new Mock<IConfiguration>();

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(x => x.Value).Returns("TestCategory");
            configurationMock.Setup(x => x.GetSection("JobsSettings:NotifyCategory")).Returns(configurationSectionMock.Object);

            service = new CalculateBonusService(repositoryMock.Object, loggerMock.Object, configurationMock.Object);
        }

        [Fact]
        public async Task GetAgentPromotion_ShouldCallRepository()
        {
            // Arrange
            const bool isTest = true;

            // Act
            await service.GetAgentPromotion(isTest);

            // Assert
            repositoryMock.Verify(r => r.ExecuteAsync(It.Is<GetAgentSettingBonusCriteria>(c => c.IsTest == isTest)), Times.Once);
        }

        [Fact]
        public async Task AddBonusTransaction_WithPromotionSettings_ShouldCallRepository()
        {
            // Arrange
            var promotions = new List<PromotionSetting>
            {
                new PromotionSetting { SiteDBGroupId = 1, AgentId = 2 },
                new PromotionSetting { SiteDBGroupId = 1, AgentId = 3 }
            };
            const short siteDbGroupId = 1;

            // Act
            await service.AddBonusTransaction(promotions, siteDbGroupId);

            // Assert
            repositoryMock.Verify(r => r.ExecuteAsync(It.IsAny<AddBonusTransactionCriteria>()), Times.Once);
        }

        [Fact]
        public async Task AddBonusTransaction_WithSizeParameter_ShouldChunkPromotions()
        {
            // Arrange
            var promotions = new List<PromotionSetting>
            {
                new PromotionSetting { SiteDBGroupId = 1, AgentId = 2 },
                new PromotionSetting { SiteDBGroupId = 1, AgentId = 3 },
                new PromotionSetting { SiteDBGroupId = 1, AgentId = 4 }
            };
            const short siteDbGroupId = 1;
            const int size = 2;

            // Act
            await service.AddBonusTransaction(promotions, siteDbGroupId, size);

            // Assert
            repositoryMock.Verify(r => r.ExecuteAsync(It.IsAny<AddBonusTransactionCriteria>()), Times.Exactly(2));
        }

        [Fact]
        public async Task AddBonusTransaction_MainMethod_WithNoPromotion_ShouldLogAndExit()
        {
            // Arrange
            repositoryMock.Setup(r => r.ExecuteAsync(It.IsAny<GetAgentSettingBonusCriteria>()))
                .Callback<GetAgentSettingBonusCriteria>(c => c.AgentPromotion = "");

            configurationMock.Setup(c => c.GetValue<string>("JobsSettings:NotifyCategory"))
                .Returns("TestCategory");

            // Act
            await service.AddBonusTransaction(false, 100);

            // Assert
            loggerMock.Verify(l => l.ErrorAsync(It.Is<string>(s => s.Contains("[Calculate Bonus] CashPromotions Count: 0")), "TestCategory"), Times.Once);
        }

        [Fact]
        public async Task AddBonusTransaction_MainMethod_WithPromotions_ShouldProcessPromotions()
        {
            // Arrange
            var promotions = new AgentPromotion
            {
                CashPromotions = new[]
                {
                    new PromotionSetting { SiteDBGroupId = 1, AgentId = 2 },
                    new PromotionSetting { SiteDBGroupId = 1, AgentId = 3 },
                    new PromotionSetting { SiteDBGroupId = 2, AgentId = 4 }
                }
            };

            repositoryMock.Setup(r => r.ExecuteAsync(It.IsAny<GetAgentSettingBonusCriteria>()))
                .Callback<GetAgentSettingBonusCriteria>(c => c.AgentPromotion = JsonSerializer.Serialize(promotions));

            configurationMock.Setup(c => c.GetValue<string>("JobsSettings:NotifyCategory"))
                .Returns("TestCategory");

            // Act
            await service.AddBonusTransaction(false, 100);

            // Assert
            loggerMock.Verify(l => l.ErrorAsync(It.Is<string>(s => s.Contains("[Calculate Bonus] CashPromotions Count: 3")), "TestCategory"), Times.Once);
            repositoryMock.Verify(r => r.ExecuteAsync(It.IsAny<AddBonusTransactionCriteria>()), Times.Exactly(2)); // One for each group
        }
    }
}