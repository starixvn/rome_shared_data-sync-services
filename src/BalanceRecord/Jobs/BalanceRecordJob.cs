using BalanceRecord.Services;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Jobs;
using Shared.Services;

namespace BalanceRecord.Jobs;

public sealed class BalanceRecordJob : IntervalBackgroundService
{
    private short siteDBGroupId;
    private int batchSize;
    private readonly IBalanceRecordService balanceRecordService;

    public BalanceRecordJob(
        ScheduleJob scheduleJob,
        ILoggerService loggerService,
        IMaintenanceService maintenanceService,
        UMClientConfiguration umClientConfiguration,
        IBalanceRecordService balanceRecordService)
        : base(scheduleJob, loggerService, maintenanceService, umClientConfiguration)
    {
        AssignParameters(scheduleJob.Parameters);
        this.balanceRecordService = balanceRecordService;
    }

    private void AssignParameters(ScheduleJobParameter[] parameters)
    {
        foreach (var item in parameters)
        {
            if (item.Key.Equals(nameof(siteDBGroupId), StringComparison.OrdinalIgnoreCase))
            {
                siteDBGroupId = Convert.ToInt16(item.Value);
            }
            else if (item.Key.Equals(nameof(batchSize), StringComparison.OrdinalIgnoreCase))
            {
                batchSize = Convert.ToInt32(item.Value);
            }
        }
    }

    public override async Task<bool> ExecuteJob()
    {
        if (await balanceRecordService.IsStopJob(scheduleJob.Name, scheduleJob.IsTest))
        {
            await loggerService.Info($"{scheduleJob.JobInfo} is set stopped from DB.");
            return false;
        }

        var lastRowInserted = await balanceRecordService.GetLastRowInserted(siteDBGroupId, scheduleJob.IsTest);
        if (!lastRowInserted.HasValue)
        {
            await loggerService.Info($"{scheduleJob.JobInfo}: LastRowInserted is NULL.");
            return false;
        }

        var transInfo = await balanceRecordService.GetDataFromSiteDB(
            scheduleJob.Name,
            lastRowInserted.Value,
            batchSize,
            scheduleJob.IsTest);
        if (transInfo == null || transInfo.Length == 0)
        {
            return false;
        }

        var errorMessage = await balanceRecordService.InsertDataIntoSiteDB(siteDBGroupId, transInfo, scheduleJob.IsTest);
        if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            await loggerService.Error(new Exception(errorMessage), scheduleJob, nameof(balanceRecordService.InsertDataIntoSiteDB), errorMessage);
            return false;
        }

        return true;
    }
}
