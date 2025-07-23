using System.Text.Json.Serialization;

namespace CashServiceBilling.Models;

public class AgentPromotion
{
    [JsonPropertyName("PromotionID")]
    public int PromotionId { get; init; }

    [JsonPropertyName("SiteDBGroupID")]
    public short SiteDBGroupId { get; init; }

    [JsonPropertyName("AgentID")]
    public int AgentId { get; init; }

    public string FromDate { get; init; }

    public string ToDate { get; init; }

    public PromotionSettings[] Details { get; init; }
}

public class PromotionSettings
{
    [JsonPropertyName("ProductCategoryID")]
    public int ProductCategoryId { get; init; }

    public string ProductList { get; init; }

    public decimal Percentage { get; init; }

    public decimal MaxPayout { get; init; }

    public decimal MinValue { get; init; }
}
