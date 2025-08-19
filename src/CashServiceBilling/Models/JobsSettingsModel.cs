namespace CashServiceBilling.Models;

public class JobsSettingsModel
{
    public string NotifyCategory { get; set; }
    public IEnumerable<int> SiteDBGroupIds { get; set; }
    public string AccumulatedCreditJobCron { get; set; }
    public string CalculateBonusJobCron { get; set; }
    public string UpdatePromotionExpiredStatusJobCron { get; set; }
    public bool IsTest { get; set; }
}
