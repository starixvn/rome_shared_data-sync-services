using System.Text.Json;
using CashServiceBilling.Criteria;
using CashServiceBilling.Models;
using Fanex.Data.Repository;
using Moq;
using Shared.Configuration;
using Shared.Services;

namespace CashServiceBilling.Services.Tests
{
    public class CalculateBonusServiceTests
    {
        private readonly Mock<IDynamicRepository> _mockRepository;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly CalculateBonusService _service;

        public CalculateBonusServiceTests()
        {
            _mockRepository = new Mock<IDynamicRepository>();
            _mockLogger = new Mock<ILoggerService>();
            _service = new CalculateBonusService(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CalculateAndAddBonusTransactions_WithNoAgentPromotions_ShouldOnlyLogAndNotProcessFurther()
        {
            // Arrange
            var jobSettings = new JobsSettingsModel { NotifyCategory = "Test", IsTest = false };
            var scheduleJob = new ScheduleJob();

            _mockRepository.Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
                .Callback<INonQueryCommand>(criteria =>
                {
                    var c = criteria as GetAgentSettingBonusCriteria;
                    c.AgentPromotion = "";
                });

            // Act
            await _service.CalculateAndAddBonusTransactions(jobSettings, scheduleJob);

            // Assert
            _mockLogger.Verify(l => l.Notify($"Start [Calculate Bonus].", jobSettings.NotifyCategory), Times.Once);
            _mockLogger.Verify(l => l.Notify("[Calculate Bonus] AgentSettingBonus Count: 0.", jobSettings.NotifyCategory), Times.Once);
            _mockLogger.Verify(l => l.Notify("[Calculate Bonus] completed.", jobSettings.NotifyCategory), Times.Once);
        }

        [Fact]
        public async Task CalculateAndAddBonusTransactions_WithAgentPromotions_ShouldProcessAllFlows()
        {
            // Arrange
            var index = 0;
            var jobSettings = new JobsSettingsModel
            {
                NotifyCategory = "Test",
                IsTest = false,
                SiteDBGroupIds = new List<int> { 1, 2 }
            };
            var scheduleJob = new ScheduleJob { Parameters = new ScheduleJobParameter[] { } };
            var agentPromotions = new[] { new AgentPromotion() };

            _mockRepository.Setup(r => r.GetAsync<BalanceUpDayAndMoveBetsDay>(It.IsAny<GetBalanceUpDayAndMoveBetsDayCriteria>()))
                .ReturnsAsync(new BalanceUpDayAndMoveBetsDay
                {
                    BalanceUpDay = DateTime.Today,
                    MoveBetsDay = DateTime.Today.AddDays(-7)
                });

            _mockRepository.Setup(r => r.FetchAsync<ProductDetail>(It.IsAny<GetProductsDetailCriteria>()))
                .ReturnsAsync(new List<ProductDetail> { new ProductDetail() });

            _mockRepository.Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
                .Callback<INonQueryCommand>(criteria =>
                {
                    if (criteria is GetBonusTransactionCriteria)
                    {
                        var c = criteria as GetBonusTransactionCriteria;
                        c.MaxCustId = 100;
                        c.MemberBonus = index < 2 ? "bonus data" : "";
                        index++;
                    }
                    else if (criteria is GetAgentSettingBonusCriteria)
                    {
                        var c = criteria as GetAgentSettingBonusCriteria;
                        c.AgentPromotion = JsonSerializer.Serialize(agentPromotions);
                    }
                });

            // Act
            await _service.CalculateAndAddBonusTransactions(jobSettings, scheduleJob);

            // Assert
            _mockRepository.Verify(r => r.ExecuteAsync(It.IsAny<CalculateCashPromotionCriteria>()), Times.Once);
            _mockRepository.Verify(r => r.ExecuteAsync(It.IsAny<AddBonusTransactionCriteria>()), Times.AtLeastOnce);
            _mockLogger.Verify(l => l.Notify("[Calculate Bonus] AgentSettingBonus Count: 1.", jobSettings.NotifyCategory), Times.Once);
        }

        [Fact]
        public async Task GetAgentSettingBonus_WhenCriteriaReturnsEmptyString_ShouldReturnEmptyArray()
        {
            // Arrange
            _mockRepository.Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
                .Callback<INonQueryCommand>(criteria =>
                {
                    var c = criteria as GetAgentSettingBonusCriteria;
                    c.AgentPromotion = "";
                });

            // Act
            var result = await _service.GetAgentSettingBonus(false);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAgentSettingBonus_WhenCriteriaReturnsData_ShouldDeserializeCorrectly()
        {
            // Arrange
            var expected = new[] { new AgentPromotion { AgentId = 123 } };
            _mockRepository.Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
                .Callback<INonQueryCommand>(criteria =>
                {
                    var c = criteria as GetAgentSettingBonusCriteria;
                    c.AgentPromotion = JsonSerializer.Serialize(expected);
                });

            // Act
            var result = await _service.GetAgentSettingBonus(false);

            // Assert
            Assert.Single(result);
            Assert.Equal(123, result[0].AgentId);
        }

        [Fact]
        public void GetMondaySundayLastWeek_ShouldReturnCorrectDates()
        {
            // Act
            var (monday, sunday) = CalculateBonusService.GetMondaySundayLastWeek();

            // Assert
            Assert.Equal(DayOfWeek.Monday, monday.DayOfWeek);
            Assert.Equal(DayOfWeek.Sunday, sunday.DayOfWeek);
            Assert.Equal(6, (sunday - monday).Days);
        }

        [Fact]
        public void GetBatchSize_WithValidParameter_ShouldReturnSpecifiedValue()
        {
            // Arrange
            var scheduleJob = new ScheduleJob
            {
                Parameters = new ScheduleJobParameter[]
                {
                    new ScheduleJobParameter { Key = "BatchSize", Value = "1000" }
                }
            };

            // Act
            var batchSize = CalculateBonusService.GetBatchSize(scheduleJob);

            // Assert
            Assert.Equal(1000, batchSize);
        }

        [Fact]
        public void GetBatchSize_WithNoParameter_ShouldReturnDefaultValue()
        {
            // Arrange
            var scheduleJob = new ScheduleJob
            {
                Parameters = new ScheduleJobParameter[] { }
            };

            // Act
            var batchSize = CalculateBonusService.GetBatchSize(scheduleJob);

            // Assert
            Assert.Equal(3000, batchSize);
        }

        [Fact]
        public async Task GetProductDetailsXml_ShouldGenerateCorrectXml()
        {
            // Arrange
            var productDetails = new List<ProductDetail>
            {
                new ProductDetail { ResourceId = 1, ProductId = 3 },
                new ProductDetail { ResourceId = 2, ProductId = 3 }
            };

            _mockRepository.Setup(r => r.FetchAsync<ProductDetail>(It.IsAny<GetProductsDetailCriteria>()))
                .ReturnsAsync(productDetails);

            // Act
            var xml = await _service.GetProductDetailsXml();

            // Assert
            Assert.StartsWith("<ROOT>", xml);
            Assert.EndsWith("</ROOT>", xml);
            Assert.Contains("ProductId=\"3\"", xml);
        }

        [Fact]
        public async Task AddBonusTransactionsFlow_ShouldProcessAllSiteDbGroupIds()
        {
            // Arrange
            var jobSettings = new JobsSettingsModel
            {
                SiteDBGroupIds = new List<int> { 1, 2 },
                NotifyCategory = "Test"
            };

            var callCount = 0;
            _mockRepository.Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
                .Callback<INonQueryCommand>(criteria =>
                {
                    if (criteria is GetBonusTransactionCriteria)
                    {
                        var c = criteria as GetBonusTransactionCriteria;
                        callCount++;
                        c.MaxCustId = 100;
                        // Return data for first call only for each site
                        c.MemberBonus = callCount % 2 == 1 ? "bonus data" : "";
                    }
                });

            // Act
            await _service.AddBonusTransactionsFlow(jobSettings);

            // Assert
            _mockRepository.Verify(r => r.ExecuteAsync(It.IsAny<GetBonusTransactionCriteria>()), Times.Exactly(4));
            _mockRepository.Verify(r => r.ExecuteAsync(It.IsAny<AddBonusTransactionCriteria>()), Times.Exactly(2));
        }

        [Fact]
        public async Task AddBonusTransactions_ReturnsCorrectBonusChanged()
        {
            // Arrange
            _mockRepository.Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
                .Callback<INonQueryCommand>(criteria =>
                {
                    var c = criteria as AddBonusTransactionCriteria;
                    c.IsBonusChanged = true;
                });

            // Act
            var result = await _service.AddBonusTransactions("test bonus", 1);

            // Assert
            Assert.True(result);
        }
    }
}