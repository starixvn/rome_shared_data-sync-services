using System.Text.Json.Serialization;

namespace CashServiceBilling.Models;

public class CashAccumCredit
{
    [JsonPropertyName("CustID")]
    public int CustId { get; init; }

    [JsonPropertyName("recommend")]
    public int Recommend { get; init; }

    [JsonPropertyName("mrecommend")]
    public int Mrecommend { get; init; }

    [JsonPropertyName("srecommend")]
    public int Srecommend { get; init; }

    public decimal AccumCreditReturn { get; init; }

    public DateTime ModifiedDate { get; init; }

    public DateTime RepUpdateTime { get; init; }

    [JsonPropertyName("CustDBGroupID")]
    public short CustDBGroupId { get; init; }
}