namespace Shared.Configuration;

public sealed class ScheduleJob
{
    public string Name { get; init; } = default!;

    public ScheduleJobParameter[] Parameters { get; init; } = default!;

    public int DelayExecutionTimeInSeconds { get; init; }

    public bool IsActive { get; init; }

    public bool IsTest { get; set; }

    public int SettingStatus { get; set; }
    public string JobInfo
        => $"[{Name}{(IsTest ? " - UAT" : string.Empty)}]";
}
