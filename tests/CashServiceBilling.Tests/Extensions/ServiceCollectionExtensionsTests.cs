using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CashServiceBilling.Extensions;
using CashServiceBilling.Services;
using Shared.Configuration;
using Xunit;
using CashServiceBilling.Jobs;

namespace CashServiceBilling.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddApplicationServices_WhenConfigurationIsValid_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "IsTest", "true" },
                { "ScheduleJobs:0:IsActive", "true" },
                { "ScheduleJobs:0:Name", "TestJob" }
            })
            .Build();

        // Act
        var result = services.AddApplicationServices(configuration);

        // Assert
        var serviceProvider = result.BuildServiceProvider();
        Assert.NotNull(serviceProvider.GetService<ScheduleJob[]>());
    }

    [Fact]
    public void AddApplicationServices_WhenScheduleJobsIsEmpty_RegistersEmptyArray()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "IsTest", "true" }
            })
            .Build();

        // Act
        var result = services.AddApplicationServices(configuration);

        // Assert
        var serviceProvider = result.BuildServiceProvider();
        var scheduleJobs = serviceProvider.GetService<ScheduleJob[]>();
        Assert.NotNull(scheduleJobs);
        Assert.Empty(scheduleJobs);
    }

    [Fact]
    public void AddApplicationServices_WhenScheduleJobsHasInactiveJobs_OnlyRegistersActiveJobs()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ScheduleJobs:0:IsActive", "true" },
                { "ScheduleJobs:0:Name", "ActiveJob" },
                { "ScheduleJobs:1:IsActive", "false" },
                { "ScheduleJobs:1:Name", "InactiveJob" }
            })
            .Build();

        // Act
        var result = services.AddApplicationServices(configuration);

        // Assert
        var serviceProvider = result.BuildServiceProvider();
        var scheduleJobs = serviceProvider.GetService<ScheduleJob[]>();
        Assert.NotNull(scheduleJobs);
        Assert.Single(scheduleJobs);
        Assert.Equal("ActiveJob", scheduleJobs[0].Name);
        Assert.False(scheduleJobs[0].IsTest);
    }
}