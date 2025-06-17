namespace CashServiceBilling.Models;

public class JobsSettingsModel
{
    public int DelayRetryInMilliSeconds { get; set; }
    public string NotifyCategory { get; set; }
    public string Environment { get; set; }
    public IEnumerable<int> SiteDBGroupIds { get; set; }
}
