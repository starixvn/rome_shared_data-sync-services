using System.Text.Json.Serialization;

namespace CashServiceBilling.Models;

public class AgentSettingPromotion
{
    [JsonPropertyName("CustID")]
    public int CustId { get; init; }

    [JsonPropertyName("recommend")]
    public int Recommend { get; init; }

    [JsonPropertyName("SiteDBGroupID")]
    public short SiteDBGroupId { get; init; }
}