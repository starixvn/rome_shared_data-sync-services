using Fanex.Data.Repository;

namespace CashServiceBilling.Criteria;

public class CalculateCashPromotionCriteria : NonQueryCommand
{
    public CalculateCashPromotionCriteria(
        string productDetails,
        DateTime moveBetsDay,
        DateTime balanceUpDay,
        string agentPromotion)
    {
        ProductDetails = productDetails;
        MoveBetsDay = moveBetsDay;
        BalanceUpDay = balanceUpDay;
        AgentPromotion = agentPromotion;
    }

    public string ProductDetails { get; }

    public DateTime MoveBetsDay { get; }

    public DateTime BalanceUpDay { get; }

    public string AgentPromotion { get; }

    public override string GetSettingKey() => "Task_CashService_CashPromotionCal";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(AgentPromotion);
}