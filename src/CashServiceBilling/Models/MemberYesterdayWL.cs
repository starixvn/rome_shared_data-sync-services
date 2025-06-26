using System.Text.Json.Serialization;

namespace CashServiceBilling.Models;

public class MemberYesterdayWL
{
    [JsonPropertyName("CustID")]
    public int CustId { get; init; }

    public decimal YesterdayWL { get; init; }
}